using bux.Controllers.Model.Api.AyetController;
using Bux.Ayet;
using Bux.Sessionn;
using Microsoft.AspNetCore.Mvc;

namespace bux.Controllers.Api
{
    [Route("api/ayet")]
    public class AyetController: Controller
    {
        public static string CLASS_NAME = typeof(AyetController).Name;

        private int addSlotId;
        private SessionService sessionService;
        private AyetService ayetService;

        public AyetController(IConfiguration configuration, SessionService sessionService, AyetService ayetService) 
        {
            this.sessionService = sessionService;
            this.ayetService = ayetService;

        }

        [HttpGet("get-offerwall-add-slot-link")]
        public async Task<ActionResult<GetOfferWallAddSlotLinkResponse>> GetOfferWallAddSlotLink()
        {
            var userId = await sessionService.GetUserId();
            var link = await ayetService.GetOfferWallAddSlotLink(userId);
            return Ok(new GetOfferWallAddSlotLinkResponse ( link: link ));
        }
    }
}
