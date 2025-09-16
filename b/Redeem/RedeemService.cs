using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Bux.Dbo;
using Bux.Dbo.Model;
using Serilog;
using MySqlConnector;

namespace bux.Redeem
{
    public class RedeemService
    {
        public static string CLASS_NAME = typeof(RedeemService).Name;

        private readonly Db db;
        private readonly Random random = new Random();
        private MySqlDataSource dataSource;

        public RedeemService(Db Db, MySqlDataSource dataSource)
        {
            this.db = Db;
            this.dataSource = dataSource;
        }

        public async Task RedeemAmmount(int userId, decimal amount)
        {
            const string METHOD_NAME = nameof(RedeemAmmount);

            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId), "Invalid user id.");

            if (amount <= 0m)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");

            var amt = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

            // Do the deduction atomically to avoid races:
            // Only decrement if there is enough balance (Amount >= amt).
            var rows = await db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE BuxEarned
                SET Amount  = Amount  - {amt},
                WHERE UserId = {userId}
                  AND Amount >= {amt};
            ");

            if (rows == 0)
            {
                Log.Error($"{CLASS_NAME}:{METHOD_NAME}(): Insufficient balance or user not found for userId={userId}, amount={amt}");
                throw new InvalidOperationException("Insufficient balance or user not found.");
            }

            // Record the redemption
            var redeemed = new BuxRedeemed
            {
                UserId = userId,
                Amount = (double)amt,
                RedeemedAt = DateTime.UtcNow
            };

            db.BuxRedeemed.Add(redeemed);
            await db.SaveChangesAsync();
        }

        public async Task RedeemAmmount1(int userId, double amount)
        {
            const string METHOD_NAME = nameof(RedeemAmmount);

            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId), "Invalid user id.");

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");

            var amt = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

            // Do the deduction atomically to avoid races:
            // Only decrement if there is enough balance (Amount >= amt).
            var rows = await db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE BuxEarned
                SET Amount1  = Amount1  - {amt}
                WHERE UserId = {userId}
                  AND Amount1 >= {amt};
            ");

            if (rows == 0)
            {
                Log.Error($"{CLASS_NAME}:{METHOD_NAME}(): Insufficient balance or user not found for userId={userId}, amount={amt}");
                throw new InvalidOperationException("Insufficient balance or user not found.");
            }

            // Record the redemption
            var redeemed = new BuxRedeemed
            {
                UserId = userId,
                Amount = (double)amt,
                RedeemedAt = DateTime.UtcNow
            };

            db.BuxRedeemed.Add(redeemed);
            await db.SaveChangesAsync();
        }

        public record GetLastRedeemers_Item(int UserId, string UserName, double ammountRedeemed, System.DateTime redeemedAt);
        public async Task<List<GetLastRedeemers_Item>> GetLastRedeemers(int count = 10)
        {
            const string METHOD_NAME = nameof(GetLastRedeemers);

            if (count <= 0) count = 10;

            var items = await db.BuxRedeemed
                .AsNoTracking()
                .Include(r => r.User) // use navigation to fetch username
                .OrderByDescending(r => r.RedeemedAt)
                .Take(count)
                .Select(r => new GetLastRedeemers_Item(
                    r.UserId ?? 0,
                    r.User != null ? (r.User.Name ?? string.Empty) : string.Empty,
                    r.Amount,
                    r.RedeemedAt
                ))
                .ToListAsync();

            return items;
        }

        public async Task RandomlyRedeemSomeUsers(int maxUsers = 5, double maxAmmount = 110)
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
                        SELECT UserId, Amount1
                        FROM BuxEarned
                        WHERE Amount1 > 0
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
                        if (cap <= 0)
                            continue;

                        // random double in [0.01, cap], rounded to 2 decimals
                        var raw = random.NextDouble() * cap;
                        var toRedeem = Math.Floor(raw);

                        if (toRedeem <= 0.0)
                            continue;

                        await RedeemAmmount1(userId, (double)toRedeem);

                        Log.Information($"{CLASS_NAME}:{METHOD_NAME}(): Redeemed: {toRedeem} R$ from userId={userId} (balance before: {currentAmount} R$).");
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
