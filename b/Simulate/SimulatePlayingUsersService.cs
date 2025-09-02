using MySqlConnector;
using Org.BouncyCastle.Bcpg;
using Serilog;

namespace bux.Simulate
{
    public class SimulatePlayingUsersService : BackgroundService
    {
        public static string CLASS_NAME = typeof(SimulatePlayingUsersService).Name;

        private MySqlDataSource dataSource;
        private readonly Random random = new Random();

        public SimulatePlayingUsersService(MySqlDataSource dataSource)
        {
            this.dataSource = dataSource;
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
                await Task.Delay(TimeSpan.FromMinutes(5), ct);
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
                await using (var selectCmd = new MySqlCommand("SELECT Id FROM BuxEarned b INNER JOIN User u ON b.UserId = u.Id WHERE u.CreatedAt IS NULL ", conn))
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

        public record ReadTopEarnersReturn(int userId, String username, double ammount);

        public async Task ReadTopEarnersByAmmount1(int limit)
        {
            const string METHOD_NAME = nameof(ReadTopEarnersByAmmount1);

            if (limit <= 0) limit = 10;

            try
            {
                await using var conn = await dataSource.OpenConnectionAsync();

                // Use backticks for `User` to avoid clashing with MySQL's USER() function.
                const string SQL = @"
                    SELECT 
                        u.Id AS UserId,
                        u.Name AS Username,
                        IFNULL(b.Amount1, 0) AS Amount1Value
                    FROM BuxEarned b
                    INNER JOIN `User` u ON u.Id = b.UserId
                    WHERE Amount1 IS NOT NULL AND Amount1 > 0
                    ORDER BY Amount1Value DESC
                    LIMIT @limit;
                ";

                await using var cmd = new MySqlCommand(SQL, conn);
                cmd.Parameters.Add("@limit", MySqlDbType.Int32).Value = limit;

                var results = new List<ReadTopEarnersReturn>();

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    int userId = reader.GetInt32(reader.GetOrdinal("UserId"));
                    string username = reader.GetString(reader.GetOrdinal("Username")).Trim();

                    double amount = reader.GetDouble(reader.GetOrdinal("Amount1Value"));

                    // Note: your record field is named 'ammount' (with two m's) — preserving that.
                    results.Add(new ReadTopEarnersReturn(userId, username, amount));
                }

                // Log a compact table
                if (results.Count == 0)
                {
                    Log.Information($"{CLASS_NAME}:{METHOD_NAME}() No earners found.");
                    return;
                }

                Log.Information($"{CLASS_NAME}:{METHOD_NAME}() Top {results.Count} by Amount1:");
                foreach (var (userId, username, ammount) in results)
                {
                    Log.Information($" - #{userId,-6} {username,-24} Amount1={ammount}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME}() Failed to read top earners.");
                throw;
            }
        }
    }

}
