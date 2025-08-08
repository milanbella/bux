using Microsoft.AspNetCore.Mvc;
using Bux.Auth;

namespace Bux.Controllers
{
    [Route("")]
    public class RootController : Controller
    {
        ApiKeyService apiKeyService;

        public RootController(ApiKeyService apiKeyService)
        {
            this.apiKeyService = apiKeyService;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Hello()
        {
            return Content("Hello", "text/plain");
        }

        [HttpGet]
        [Route("generate-api-key")]
        public async Task<IActionResult> TestGenerateApiKey()
        {
            string apiKey = apiKeyService.GenerateApiKey();
            await apiKeyService.PersistApiKey(apiKey);
            return Content($"apiKey: {apiKey}", "text/plain");
        }
    }


}
