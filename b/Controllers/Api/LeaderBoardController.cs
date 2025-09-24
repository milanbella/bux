using Microsoft.AspNetCore.Mvc;
using Bux.Controllers.Model.Api.LeaderBoardController;
using Bux.Sessionn;
using Bux.Dbo;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Bux.Dbo.Model;
using System.ComponentModel.DataAnnotations;

namespace Bux.Controllers.Api
{
    [Route("api/leaderboard")]
    public class LeaderBoardController : Controller
    {
        public static string CLASS_NAME = typeof(LeaderBoardController).Name;

        [HttpGet]
        [Route("hello")]
        public IActionResult Hello()
        {
            return Content("Hello", "text/plain");
        }

        [HttpGet("lines")]
        public async Task<ActionResult<GetLeaderBoardResponse>> GetLeaderboard(
            [FromServices] MySqlConnector.MySqlDataSource dataSource,
            [FromServices] SessionService sessionService,
            [FromServices] IConfiguration configuration,
            [FromQuery, Range(1, 1000)] int? limit)
        {
            const string METHOD_NAME = "GetLeaderboard()";

            try
            {
                int userId = await sessionService.GetUserId(); // current user id

                const int DEFAULT_LIMIT = 30;
                const int MAX_LIMIT = 1000;
                int effectiveLimit = Math.Clamp(limit ?? DEFAULT_LIMIT, 1, MAX_LIMIT);

                // Get config for URL base
                var urlFilesBase = configuration["file_store_url"];
                if (string.IsNullOrEmpty(urlFilesBase))
                {
                    Log.Error($"{CLASS_NAME}:{METHOD_NAME}: Missing configuration: file_store_url");
                    throw new Exception("Missing configuration: file_store_url");
                }

                var lines = new List<LeaderBoardLine>();

                using (var conn = await dataSource.OpenConnectionAsync())
                {
                    // 1) Get leaderboard lines (real users use Amount; fake users use Amount1)
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            SELECT
                                t.Name,
                                t.AmountValue,
                                a.path64,
                                a.file64
                            FROM
                            (
                                SELECT u.Id AS UserId, u.Name, be.Amount AS AmountValue
                                FROM BuxEarned be
                                JOIN `User` u ON be.UserId = u.Id
                                WHERE u.CreatedAt IS NOT NULL AND be.Amount > 0

                                UNION ALL

                                SELECT u.Id AS UserId, u.Name, be.Amount1 AS AmountValue
                                FROM BuxEarned be
                                JOIN `User` u ON be.UserId = u.Id
                                WHERE u.CreatedAt IS NULL AND be.Amount1 > 0
                            ) AS t
                            LEFT JOIN Avatar a ON a.UserId = t.UserId
                            ORDER BY t.AmountValue DESC
                            LIMIT @limit;
                            ";
                        cmd.Parameters.AddWithValue("@limit", effectiveLimit);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string name = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                                double amount = reader.GetDouble(1);

                                string? avatarPath64 = reader.IsDBNull(2) ? null : reader.GetString(2);
                                string? avatarFile64 = reader.IsDBNull(3) ? null : reader.GetString(3);

                                string? avatarUrl = null;
                                if (!string.IsNullOrEmpty(avatarPath64) && !string.IsNullOrEmpty(avatarFile64))
                                {
                                    avatarUrl = $"{urlFilesBase}/{avatarPath64}/{avatarFile64}";
                                }

                                lines.Add(new LeaderBoardLine(name, amount, avatarUrl));
                            }
                        }
                    }

                    // 2) Get the current user's effective amount (Amount for real, Amount1 for fake)
                    int myPlace = 0;
                    double? myAmount = null;

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            SELECT
                                CASE WHEN u.CreatedAt IS NULL THEN be.Amount1 ELSE be.Amount END AS AmountValue
                            FROM BuxEarned be
                            JOIN `User` u ON u.Id = be.UserId
                            WHERE be.UserId = @userId
                            LIMIT 1;
                        ";
                        cmd.Parameters.AddWithValue("@userId", userId);

                        var result = await cmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            myAmount = Convert.ToDouble(result);
                        }
                    }

                    // 3) Compute place among everyone using the same unified metric
                    if (myAmount.HasValue)
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                                SELECT COUNT(*) FROM
                                (
                                    SELECT CASE WHEN u.CreatedAt IS NULL THEN be.Amount1 ELSE be.Amount END AS AmountValue
                                    FROM BuxEarned be
                                    JOIN `User` u ON u.Id = be.UserId
                                ) AS x
                                WHERE x.AmountValue > @amount;
                            ";
                            cmd.Parameters.AddWithValue("@amount", myAmount.Value);

                            var result = await cmd.ExecuteScalarAsync();
                            myPlace = Convert.ToInt32(result) + 1;
                        }
                    }

                    return StatusCode(200, new GetLeaderBoardResponse(
                        lines: lines,
                        myPlace: myPlace
                    ));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME} - internal error");
                return StatusCode(500, "internal error");
            }
        }


    }
}
