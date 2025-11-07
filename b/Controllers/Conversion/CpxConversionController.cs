using System.Globalization;
using Bux.Cpx;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bux.Conversion
{
    [Route("conversion/cpx")]
    public class CpxConversionController : Controller
    {
        private readonly CpxService cpxService;

        public CpxConversionController(CpxService cpxService)
        {
            this.cpxService = cpxService;
        }

        [HttpGet("")]
        public IActionResult ProcessCpxOfferWallCallback()
        {
            OfferWallCallbackData callbackData = MapRequestToCallbackData(Request?.Query);
            var result = cpxService.ProcessOfferWallCallback(callbackData);
            Response.StatusCode = result.httpStatus;
            return Content(result.responseText ?? string.Empty, "text/plain");
        }

        private static OfferWallCallbackData MapRequestToCallbackData(IQueryCollection? query)
        {
            if (query == null)
            {
                return new OfferWallCallbackData();
            }

            string? Get(string key) => query.TryGetValue(key, out var value) ? value.ToString() : null;

            int? ParseInt(string key)
            {
                string? value = Get(key);
                return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
            }

            decimal? ParseDecimal(string key)
            {
                string? value = Get(key);
                return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
            }

            return new OfferWallCallbackData
            {
                Status = ParseInt("status"),
                TransactionId = Get("trans_id"),
                UserId = Get("user_id"),
                SubId1 = Get("sub_id_1") ?? Get("sub_id"),
                SubId2 = Get("sub_id_2"),
                AmountLocal = ParseDecimal("amount_local"),
                AmountUsd = ParseDecimal("amount_usd"),
                OfferId = Get("offer_id"),
                Hash = Get("hash") ?? Get("secure_hash"),
                IpClick = Get("ip_click"),
                Type = Get("type")
            };
        }
    }
}
