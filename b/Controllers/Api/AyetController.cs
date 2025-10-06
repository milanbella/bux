using bux.Controllers.Model.Api.AyetController;
using Bux.Ayet;
using Bux.Sessionn;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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

        [HttpGet("get-conversions")]
        public async Task<ActionResult<ConversionsListResponse>> GetConversions()
		{
            var userId = await sessionService.GetUserId();

            var conversions = await ayetService.GetConversions(userId);

            var response = new ConversionsListResponse(
                conversions
                    .Select(c => new ConversionsListItem(
                        c.ReceivedAt,
                        c.TransactionId,
                        c.PayoutUsd,
                        c.CurrencyAmount,
                        c.CurrencyIdentifier,
                        c.CurrencyConversionRate,
                        c.UserId,
                        c.ExternalIdentifier,
                        c.PlacementIdentifier,
                        c.AdslotId,
                        c.OfferId,
                        c.OfferName,
                        c.IsChargeback,
                        c.ChargebackReason,
                        c.ChargebackDate))
                    .ToList());

            return Ok(response);
		}
    }
}
