using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bux.Controllers.Model.Api.UserController;
using Bux.Dbo;
using Bux.Sessionn;

namespace Bux.Controllers.Api
{
    [Route("api/user")]
    public class UserController: Controller
    {
        private SessionService sessionService;

        public UserController(SessionService sessionService)
        {
            this.sessionService = sessionService;
        }

        [HttpGet]
        [Route("hello")]
        public IActionResult Hello()
        {
            return Content("Hello", "text/plain");
        }

        [HttpGet("get-user")]
        public async Task<ActionResult<GetUserResponse>> GetUser()
        {
            var user = await sessionService.GetLoggedInUser();
            if (user == null)
            {
                return Unauthorized("No user logged in");
            }
            var getUserResponse = new GetUserResponse(user.Name, user.Email, user.FirstName, user.LastName);

            return Ok(getUserResponse);
        }

        [HttpGet("get-total-bux-earned")]
        public async Task<ActionResult<GetTotalBuxEarnedResponse>> GetTotalBuxEarned([FromServices] Db db)
        {
            var user = await sessionService.GetLoggedInUser();
            if (user == null)
            {
                return Unauthorized("No user logged in");
            }

            var buxEarned = await db.BuxEarned.FirstOrDefaultAsync(b => b.UserId == user.Id);
            double total = buxEarned?.Amount ?? 0;

            var response = new GetTotalBuxEarnedResponse(total);
            return Ok(response);
        }
    }
}
