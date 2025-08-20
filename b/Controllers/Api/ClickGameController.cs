using Microsoft.AspNetCore.Mvc;
using Bux.Controllers.Model.Api.ClickGameController;
using Bux.Sessionn;
using Bux.Dbo;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Bux.Dbo.Model;

namespace Bux.Controllers.Api
{
    [Route("api/click-game")]
    public class ClickGameController: Controller
    {
        public static string CLASS_NAME = typeof(ClickGameController).Name;

        private SessionService sessionService;

        private const int CLICKS_TO_EARN_BUX = 10;

        public ClickGameController()
        {
        }

        [HttpGet]
        [Route("hello")]
        public IActionResult Hello()
        {
            return Content("Hello", "text/plain");
        }

        [HttpPost("click")]
        public async Task<ActionResult<ClickResponse>> Click([FromServices] Db db, [FromServices] SessionService sessionService)
        {
            const string METHOD_NAME = "Click()";

            using var transaction = db.Database.BeginTransaction();
            try
            {
                int userId = await sessionService.GetUserId();

                // read the click game state for the user
                var clickGame = await db.ClickGame.FirstOrDefaultAsync(cg => cg.UserId == userId);
                if (clickGame == null)
                {
                    // if no state exists, create a new one
                    clickGame = new ClickGame
                    {
                        UserId = userId,
                        Clicks = 1,
                    };
                    db.ClickGame.Add(clickGame);
                }
                else
                {
                    // if state exists, increment the clicks
                    clickGame.Clicks++;
                    db.ClickGame.Update(clickGame);
                }

                double buxAmount = 0;
                if (clickGame.Clicks  == CLICKS_TO_EARN_BUX)
                {
                    // read BuxEarned for the user
                    var buxEarned = await db.BuxEarned.FirstOrDefaultAsync(b => b.UserId == userId);
                    if (buxEarned == null)
                    {
                        // if no BuxEarned exists, create a new one
                        buxEarned = new BuxEarned
                        {
                            UserId = userId,
                            Amount = 10
                        };
                        db.BuxEarned.Add(buxEarned);
                    }
                    else
                    {
                        // if BuxEarned exists, increment the amount
                        buxEarned.Amount += 1;
                        db.BuxEarned.Update(buxEarned);
                    }

                    buxAmount = 1;
                    clickGame.Clicks = 0; // reset clicks after earning Bux
                    db.ClickGame.Update(clickGame);
                }

                await db.SaveChangesAsync();
                transaction.Commit();

                return await Task.FromResult(StatusCode(200, new ClickResponse(
                        buxAmount: buxAmount,
                        clicksCount: CLICKS_TO_EARN_BUX - clickGame.Clicks

                    )));

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME} - internal error");
                return await Task.FromResult(StatusCode(500, "internal error"));
            }
        }

        [HttpGet("game-state")]
        public async Task<ActionResult<GameState>> GameState([FromServices] Db db, [FromServices] SessionService sessionService)
        {
            const string METHOD_NAME = "GameState()";
            try
            {
                int userId = await sessionService.GetUserId();
                
                var clickGame = await db.ClickGame.FirstOrDefaultAsync(cg => cg.UserId == userId);
                if (clickGame == null)
                {
                    // if no state exists, create a new one
                    clickGame = new ClickGame
                    {
                        UserId = userId,
                        Clicks = 0,
                    };
                    db.ClickGame.Add(clickGame);
                    await db.SaveChangesAsync();
                }

                return await Task.FromResult(StatusCode(200, new GameState(
                    clicksCount: CLICKS_TO_EARN_BUX - clickGame.Clicks
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
