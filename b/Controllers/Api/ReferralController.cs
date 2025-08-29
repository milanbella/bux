using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bux.Controllers.Model.Api.UserController;
using Bux.Dbo;
using Bux.Dbo.Model;
using Bux.Sessionn;
using Serilog;

namespace Bux.Controllers.Api
{
    [Route("referral")]
    public class ReferralController : Controller
    {
        public static string CLASS_NAME = typeof(ReferralController).Name;

        private SessionService sessionService;

        public ReferralController(SessionService sessionService)
        {
            this.sessionService = sessionService;
        }

        [HttpGet]
        [Route("hello")]
        public IActionResult Hello()
        {
            return Content("Hello", "text/plain");
        }

        [HttpGet("")]
        public ActionResult RootPath()
        {
            string? code = Request.Query["code"];
            if (string.IsNullOrEmpty(code))
            {
                return Redirect("/");
            }
            else
            {
                return Redirect($"/account.html?referral_code={code}");
            }
        }
    }

}
