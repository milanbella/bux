using Bux.Controllers.Model.Api1.EarnersController;
using Microsoft.AspNetCore.Mvc;
using Bux.Simulate;
using MySqlConnector;
using Serilog;
using bux.Redeem;

namespace Bux.Controllers.Api1
{
    [Route("api1/earners")]
    public class EarnersController : Controller
    {
        public static string CLASS_NAME = typeof(EarnersController).Name;

        MySqlDataSource dataSource;

        public EarnersController(MySqlDataSource dataSource)
        {
            this.dataSource = dataSource;
        }

        [HttpGet]
        [Route("hello")]
        public IActionResult Hello()
        {
            return Content("Hello", "text/plain");
        }

        [HttpGet]
        [Route("top")]
        public async Task<ActionResult<Earner>> GetTopEarners()
        {
            const string METHOD_NAME = nameof(GetTopEarners);
            try 
            {
                var earners = await SimulatePlayingUsersService.ReadTopEarnersByAmmount(dataSource, 10);
                return Ok(new TopEarnersResponse(earners));
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME}() - Exception: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Route("last-redeemers")]
        public async Task<ActionResult<LastRedeemersResponse>> GetLastRedeemers([FromServices] RedeemService redeemService, [FromQuery] int count = 10)
        {
            const string METHOD_NAME = nameof(GetLastRedeemers);
            try
            {
                if (count <= 0) count = 10;

                var items = await redeemService.GetLastRedeemers(count);

                // map service record -> API DTO
                var dto = items.Select(x => new LastRedeemerDto(
                    x.UserId,
                    x.UserName,
                    x.ammountRedeemed,   // keeping your original field spelling
                    x.redeemedAt
                )).ToList();

                return Ok(new LastRedeemersResponse(dto));
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME}() - Exception: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
