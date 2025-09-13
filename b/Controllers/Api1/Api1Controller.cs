using Bux.Controllers.Model.Api1.EarnersController;
using Microsoft.AspNetCore.Mvc;
using Bux.Simulate;
using MySqlConnector;
using Serilog;
using Bux.Dbo;
using Bux.Sessionn;

namespace Bux.Controllers.Api1
{
    [Route("api1")]
    public class ApiController : Controller
    {
        public static string CLASS_NAME = typeof(ApiController).Name;

        MySqlDataSource dataSource;

        public ApiController(MySqlDataSource dataSource)
        {
            this.dataSource = dataSource;
        }

        [HttpGet]
        [Route("hello")]
        public IActionResult Hello()
        {
            return Content("Hello", "text/plain");
        }


        [HttpPost]
        [Route("discord-click")]
        public async Task<ActionResult> DiscordClick(
            [FromServices] Db _ /* unused here */,
            [FromServices] SessionService sessionService)
        {
            const string METHOD_NAME = nameof(DiscordClick);

            try
            {
                long? sessionId = sessionService.GetSessionId();
                if (sessionId is null)
                    return Ok(new { ok = true, earned = false, reason = "no-session" });

                await using var conn = await dataSource.OpenConnectionAsync();
                await using var tx = await conn.BeginTransactionAsync();

                // 1) Resolve UserId for this session (only if not expired)
                int? userId = null;
                const string sqlGetUserId = @"
                    SELECT UserId
                    FROM SessionUser
                    WHERE SessionId = @sessionId
                      AND ExpiresAt > NOW()
                    LIMIT 1;";

                await using (var cmd = new MySqlCommand(sqlGetUserId, conn, (MySqlTransaction)tx))
                {
                    cmd.Parameters.AddWithValue("@sessionId", sessionId.Value);
                    var obj = await cmd.ExecuteScalarAsync();
                    if (obj != null && obj != DBNull.Value)
                        userId = Convert.ToInt32(obj);
                }

                if (userId is null)
                {
                    await tx.CommitAsync(); // nothing to do
                    return Ok(new { ok = true, earned = false, reason = "no-user-for-session" });
                }

                // 2) Mark first click (idempotent): update only if previously NULL/0
                // RowsAffected == 1 means it WAS the first click.
                int rows;
                const string sqlMarkClicked = @"
                    UPDATE `User`
                    SET IsDiscordClick = 1
                    WHERE Id = @userId
                      AND (IsDiscordClick IS NULL OR IsDiscordClick = 0);";

                await using (var cmd = new MySqlCommand(sqlMarkClicked, conn, (MySqlTransaction)tx))
                {
                    cmd.Parameters.AddWithValue("@userId", userId.Value);
                    rows = await cmd.ExecuteNonQueryAsync();
                }

                if (rows == 0)
                {
                    // Already clicked before: nothing to award
                    await tx.CommitAsync();
                    return Ok(new { ok = true, earned = false, reason = "already-clicked" });
                }

                // 3) First-time award: +1 Bux (UPSERT)
                // NOTE: For best performance, ensure a UNIQUE INDEX on BuxEarned(UserId).
                const string sqlUpsertBux = @"
                    INSERT INTO BuxEarned (UserId, Amount, Amount1)
                    VALUES (@userId, 1, 0)
                    ON DUPLICATE KEY UPDATE Amount = Amount + 1;";

                await using (var cmd = new MySqlCommand(sqlUpsertBux, conn, (MySqlTransaction)tx))
                {
                    cmd.Parameters.AddWithValue("@userId", userId.Value);
                    await cmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
                return Ok(new { ok = true, earned = true });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"{CLASS_NAME}:{METHOD_NAME}() - Exception: {ex.Message}");
                return Ok(new { ok = false });
            }
        }

    }
}
