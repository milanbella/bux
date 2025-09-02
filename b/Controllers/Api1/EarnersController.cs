using Bux.Controllers.Model.Api1.EarnersController;
using Microsoft.AspNetCore.Mvc;
using Bux.Simulate;
using MySqlConnector;
using Serilog;

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
    }
}
