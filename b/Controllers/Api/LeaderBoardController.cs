using Microsoft.AspNetCore.Mvc;
using Bux.Controllers.Model.Api.GuessGameController;
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
        public async Task<ActionResult<ClickResponse>> Click([FromBody] ClickRequest request, [FromServices] Db db, [FromServices] SessionService sessionService)
        {
            const string METHOD_NAME = "Click()";

            using var transaction = db.Database.BeginTransaction();
            try
            {
                int userId = await sessionService.GetUserId();

                // read the click game state for the user
                var guessGame = await db.GuessGame.FirstOrDefaultAsync(cg => cg.UserId == userId);
                if (guessGame == null)
                {
                    // if no state exists, create a new one
                    guessGame = new GuessGame
                    {
                        UserId = userId,
                        number = randomNumber(),
                    };
                    db.GuessGame.Add(guessGame);
                }

                double buxAmount = 0;
                bool isMatch = false;
                if (guessGame.number  == request.guessNumber)
                {
                    // read BuxEarned for the user
                    var buxEarned = await db.BuxEarned.FirstOrDefaultAsync(b => b.UserId == userId);
                    if (buxEarned == null)
                    {
                        // if no BuxEarned exists, create a new one
                        buxEarned = new BuxEarned
                        {
                            UserId = userId,
                            Amount = 1
                        };
                        db.BuxEarned.Add(buxEarned);

                        guessGame.number = randomNumber();
                        db.GuessGame.Update(guessGame);
                    }
                    else
                    {
                        // if BuxEarned exists, increment the amount
                        buxEarned.Amount += 1;
                        db.BuxEarned.Update(buxEarned);
                    }
                    buxAmount = 1;
                    isMatch = true;
                }

                await db.SaveChangesAsync();
                transaction.Commit();

                return await Task.FromResult(StatusCode(200, new ClickResponse(
                        buxAmount: buxAmount,
                        isMatch: isMatch
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

                return await Task.FromResult(StatusCode(200, new GameState(
                    min: MIN,
                    max: MAX
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
