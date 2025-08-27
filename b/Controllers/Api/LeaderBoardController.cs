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
    public class LeaderBoardController: Controller
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
                int userId = await sessionService.GetUserId(); // my user id

                const int DEFAULT_LIMIT = 100;
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
                    // Get leaderboard lines
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"
                            SELECT u.Name, be.Amount, a.path64, a.file64
                            FROM BuxEarned be
                            LEFT JOIN User u ON be.UserId = u.Id
                            LEFT JOIN Avatar a ON a.UserId = u.Id
                            ORDER BY be.Amount DESC
                            LIMIT @limit
                        ";
                        cmd.Parameters.AddWithValue("@limit", effectiveLimit);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string name = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                                double amount = reader.GetDouble(1);
                                string avatarPath64 = reader.IsDBNull(2) ? null : reader.GetString(2);
                                string avatarFile64 = reader.IsDBNull(3) ? null : reader.GetString(3);
                                lines.Add(new LeaderBoardLine(name, amount, $"{urlFilesBase}/{avatarPath64}/{avatarFile64}"));
                            }
                        }
                    }

                    // Get user's place
                    int myPlace = 0;
                    double? myAmount = null;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT Amount FROM BuxEarned WHERE UserId = @userId LIMIT 1";
                        cmd.Parameters.AddWithValue("@userId", userId);
                        var result = await cmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            myAmount = Convert.ToDouble(result);
                        }
                    }
                    if (myAmount.HasValue)
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "SELECT COUNT(*) FROM BuxEarned WHERE Amount > @amount";
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
