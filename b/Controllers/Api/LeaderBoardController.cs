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
        public async Task<ActionResult<GetLeaderBoardResponse>> GetLeaderboard(Db db, [FromServices] SessionService sessionService)
        {
            const string METHOD_NAME = "GetLeaderboard()";

            try
            {
                int userId = await sessionService.GetUserId(); // my user id


            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME} - internal error");
                return await Task.FromResult(StatusCode(500, "internal error"));
            }
        }

    }
}
