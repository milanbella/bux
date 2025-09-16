using MySqlConnector;
using Org.BouncyCastle.Bcpg;
using Serilog;
using Bux.Controllers.Model.Api1.EarnersController;
using bux.Redeem;

namespace Bux.Simulate
{
    public class SimulatePlayingUsersService : BackgroundService
    {
        public static string CLASS_NAME = typeof(SimulatePlayingUsersService).Name;

        private MySqlDataSource dataSource;
        private readonly Random random = new Random();
        private readonly RedeemService redeemService;

        public SimulatePlayingUsersService(MySqlDataSource dataSource)
        {
            this.dataSource = dataSource;
            this.redeemService = redeemService;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            const string METHOD_NAME = nameof(ExecuteAsync);

            // loop until app shuts down
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    Log.Information($"{CLASS_NAME}:{METHOD_NAME}() Background task started at: {DateTimeOffset.Now}");

                    // 🔹 Do your async work here
                    await IncrementAmount1RandomlyAsync(ct);

                    Log.Information($"{CLASS_NAME}:{METHOD_NAME}() Background task finished at: {DateTimeOffset.Now}");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME}() Error while running background task");
                }

                // wait 5 minutes before running again
                //await Task.Delay(TimeSpan.FromMinutes(5), ct);
                await Task.Delay(TimeSpan.FromSeconds(5), ct);

                await redeemService.RandomlyRedeemSomeUsers();
            }
        }

        private async Task IncrementAmount1RandomlyAsync(CancellationToken ct)
        {
            const string METHOD_NAME = nameof(IncrementAmount1RandomlyAsync);

            await using var conn = await dataSource.OpenConnectionAsync(ct);

            try
            {
                // 1) Read all IDs
                var ids = new List<int>();
                // user.CreatedAt IS NULL means user is fake/simulated
                await using (var selectCmd = new MySqlCommand("SELECT u.Id as userId FROM BuxEarned b LEFT JOIN User u ON b.UserId = u.Id WHERE u.CreatedAt IS NULL ", conn))
                {
                    await using var reader = await selectCmd.ExecuteReaderAsync(ct);
                    while (await reader.ReadAsync(ct))
                    {
                        if (!reader.IsDBNull(0))
                            ids.Add(reader.GetInt32(0));
                    }
                }

                if (ids.Count == 0)
                {
                    Log.Information($"{CLASS_NAME}:{METHOD_NAME}() No rows in BuxEarned.");
                    return;
                }

                // 2) Prepare UPDATE command (Amount1 may be NULL => treat as 0)
                await using var updateCmd = new MySqlCommand(
                    "UPDATE BuxEarned SET Amount1 = IFNULL(Amount1, 0) + @delta WHERE Id = @id",
                    conn
                );
                var deltaParam = updateCmd.Parameters.Add("@delta", MySqlDbType.Double);
                var idParam = updateCmd.Parameters.Add("@id", MySqlDbType.Int32);
                updateCmd.Prepare();

                int updated = 0;

                foreach (var id in ids)
                {
                    int n = random.Next(0, 4);
                    double delta = n;

                    deltaParam.Value = delta;
                    idParam.Value = id;

                    updated += await updateCmd.ExecuteNonQueryAsync(ct);
                }

                Log.Information($"{CLASS_NAME}:{METHOD_NAME}() Updated {updated} rows in BuxEarned.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME}() Failed to update BuxEarned; transaction rolled back.");
                throw;
            }
        }


        public static async Task<List<Earner>> ReadTopEarnersByAmmount(MySqlDataSource dataSource, int limit)
        {
            const string METHOD_NAME = nameof(ReadTopEarnersByAmmount);

            if (limit <= 0) limit = 10;

            try
            {
                await using var conn = await dataSource.OpenConnectionAsync();

                const string SQL = @"
                SELECT *
                FROM (
                    SELECT
                        u.Id   AS UserId,
                        u.Name AS Username,
                        b.Amount AS AmountValue
                    FROM `user` u
                    JOIN buxearned b ON b.UserId = u.Id
                    WHERE u.CreatedAt IS NOT NULL
                          AND b.Amount > 0

                UNION ALL

                SELECT
                    u.Id   AS UserId,
                    u.Name AS Username,
                    b.Amount1 AS AmountValue
                FROM `user` u
                JOIN buxearned b ON b.UserId = u.Id
                WHERE u.CreatedAt IS NULL
                     AND b.Amount1 > 0
                ) AS t
                ORDER BY t.AmountValue DESC
                LIMIT @limit;

                ";

                await using var cmd = new MySqlCommand(SQL, conn);
                cmd.Parameters.Add("@limit", MySqlDbType.Int32).Value = limit;

                var results = new List<Earner>();

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    int userId = reader.GetInt32(reader.GetOrdinal("UserId"));

                    string username;
                    var usernameOrdinal = reader.GetOrdinal("Username");
                    username = reader.IsDBNull(usernameOrdinal) ? "(no name)" : reader.GetString(usernameOrdinal).Trim();

                    double amount = reader.GetDouble(reader.GetOrdinal("AmountValue"));

                    results.Add(new Earner(userId, username, amount));
                }

                List<Earner> earners = new List<Earner>();
                foreach (var (userId, username, ammount) in results)
                {
                    earners.Add(new Earner(userId, username, ammount));
                }

                return earners;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME}() Failed to read top earners.");
                throw;
            }
        }

        public async Task RandomlyRedeemSomeUsers(int maxUsers = 5, double maxAmmount = 5.0)
        {
            const string METHOD_NAME = nameof(RandomlyRedeemSomeUsers);

            if (maxUsers <= 0) maxUsers = 5;
            if (maxAmmount <= 0) maxAmmount = 5.0;

            try
            {
                // 1) Pick random candidates directly in MySQL for performance.
                //    We fetch UserId + current Amount so we can clamp the random redemption.
                var candidates = new List<(int UserId, double Amount)>();

                await using (var conn = await dataSource.OpenConnectionAsync())
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT UserId, Amount
                        FROM BuxEarned
                        WHERE Amount > 0
                        ORDER BY RAND()
                        LIMIT @limit;
                    ";
                    cmd.Parameters.AddWithValue("@limit", maxUsers);

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var userId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        var amount = reader.IsDBNull(1) ? 0.0 : reader.GetDouble(1);
                        if (userId > 0 && amount > 0.0)
                            candidates.Add((userId, amount));
                    }
                }

                if (candidates.Count == 0)
                {
                    Log.Information($"{CLASS_NAME}:{METHOD_NAME}(): No users with positive balance found.");
                    return;
                }
                Log.Information($"{CLASS_NAME}:{METHOD_NAME}(): redeeming {candidates.Count} users ...");

                // 2) For each candidate, redeem a random amount up to their available balance (and maxAmmount).
                foreach (var (userId, currentAmount) in candidates)
                {
                    try
                    {
                        var cap = Math.Min(maxAmmount, currentAmount);
                        if (cap <= 0.0)
                            continue;

                        // random double in [0.01, cap], rounded to 2 decimals
                        var raw = 0.01 + (random.NextDouble() * Math.Max(0.0, cap - 0.01));
                        var toRedeem = Math.Round(raw, 2, MidpointRounding.AwayFromZero);

                        if (toRedeem <= 0.0)
                            continue;

                        await RedeemAmmount(userId, (decimal)toRedeem);

                        Log.Information($"{CLASS_NAME}:{METHOD_NAME}(): Redeemed {toRedeem:F2} from userId={userId} (balance before ~{currentAmount:F2}).");
                    }
                    catch (Exception ex)
                    {
                        // Continue with other users even if one fails.
                        Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME}(): Failed redeem for userId={userId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME}(): Unexpected error.");
                throw; // rethrow to let caller handle if needed
            }
        }

    }

}
