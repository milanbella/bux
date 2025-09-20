using MySqlConnector;
using Org.BouncyCastle.Bcpg;
using Serilog;
using Bux.Controllers.Model.Api1.EarnersController;
using bux.Redeem;

namespace Bux.Simulate
{
    public class SimulateAmountService : BackgroundService
    {
        public static string CLASS_NAME = typeof(SimulateAmountService).Name;

        private MySqlDataSource dataSource;
        private readonly Random random = new Random();
        private readonly IServiceScopeFactory scopes;

        public SimulateAmountService(IServiceScopeFactory scopes, MySqlDataSource dataSource)
        {
            this.dataSource = dataSource;
            this.scopes = scopes;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            const string METHOD_NAME = nameof(ExecuteAsync);

            using var scope = scopes.CreateScope();
            var redeemService = scope.ServiceProvider.GetRequiredService<RedeemService>();

            // loop until app shuts down
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    Log.Information($"{CLASS_NAME}:{METHOD_NAME}() Background task started at: {DateTimeOffset.Now}");

                    await IncrementAmount1RandomlyAsync(ct);

                    Log.Information($"{CLASS_NAME}:{METHOD_NAME}() Background task finished at: {DateTimeOffset.Now}");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME}() Error while running background task");
                }

                await Task.Delay(TimeSpan.FromMinutes(60), ct);

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
                await using (var selectCmd = new MySqlCommand("SELECT u.Id as userId FROM BuxEarned b LEFT JOIN User u ON b.UserId = u.Id WHERE u.CreatedAt IS NULL AND b.Amount1 > 0", conn))
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
                    int n = random.Next(0, 3);
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


    }

}
