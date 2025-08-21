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
            [FromServices] Db db,
            [FromServices] SessionService sessionService,
            [FromQuery, Range(1, 1000)] int? limit)
        {
            const string METHOD_NAME = "GetLeaderboard()";

            try
            {
                int userId = await sessionService.GetUserId(); // my user id

                const int DEFAULT_LIMIT = 100;
                const int MAX_LIMIT = 1000;
                int effectiveLimit = Math.Clamp(limit ?? DEFAULT_LIMIT, 1, MAX_LIMIT);

                var topBux = await db.BuxEarned
                    .Include(be => be.User)
                    .OrderByDescending(be => be.Amount)
                    .Take(effectiveLimit)
                    .ToListAsync();

                var lines = topBux
                    .Select(be => new LeaderBoardLine(be.User?.Name ?? string.Empty, be.Amount))
                    .ToList();

                var userEntry = await db.BuxEarned.FirstOrDefaultAsync(be => be.UserId == userId);

                int myPlace = 0;
                if (userEntry != null)
                {
                    myPlace = await db.BuxEarned.CountAsync(be => be.Amount > userEntry.Amount) + 1;
                }

                return await Task.FromResult(StatusCode(200, new GetLeaderBoardResponse(
                    lines: lines,
                    myPlace: myPlace
                )));

            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME} - internal error");
                return await Task.FromResult(StatusCode(500, "internal error"));
            }
        }

    }
}
