using System;
using System.Globalization;
using Bux.Ayet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bux.Conversion
{
    [Route("conversion/ayet")]
    public class AyetConversionController : Controller
    {
        private readonly AyetService ayeService;

        public AyetConversionController(AyetService ayetService)
        {
            this.ayeService = ayetService;
        }

        [HttpGet("")]
        public IActionResult ProcessAyeOfferWallCallback()
        {
            OfferWallCallbackData callbackData = MapRequestToCallbackData(Request?.Query);
            var result = ayeService.ProcessOfferWallCallback(callbackData);
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

            long? ParseLong(string key)
            {
                string? value = Get(key);
                return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
            }

            int ParseChargeback()
            {
                string? value = Get("is_chargeback");
                return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
            }

            return new OfferWallCallbackData
            {
                TransactionId = Get("transaction_id"),
                PayoutUsd = ParseDecimal("payout_usd"),
                CurrencyAmount = ParseDecimal("currency_amount"),
                ExternalIdentifier = Get("external_identifier"),
                UserId = ParseInt("user_id"),
                PlacementIdentifier = Get("placement_identifier"),
                AdslotId = ParseInt("adslot_id"),
                SubId = Get("sub_id"),
                Ip = Get("ip"),
                OfferId = ParseInt("offer_id"),
                OfferName = Get("offer_name"),
                DeviceUuid = Get("device_uuid"),
                DeviceMake = Get("device_make"),
                DeviceModel = Get("device_model"),
                AdvertisingId = Get("advertising_id"),
                Sha1AndroidId = Get("sha1_android_id"),
                Sha1Imei = Get("sha1_imei"),
                IsChargeback = ParseChargeback(),
                ChargebackReason = Get("chargeback_reason"),
                ChargebackDate = Get("chargeback_date"),
                EventName = Get("event_name"),
                EventValue = ParseDecimal("event_value"),
                TaskName = Get("task_name"),
                TaskUuid = Get("task_uuid"),
                CurrencyIdentifier = Get("currency_identifier"),
                CurrencyConversionRate = ParseDecimal("currency_conversion_rate"),
                ClickDate = Get("click_date"),
                CallbackTs = ParseLong("callback_ts"),
                Custom1 = Get("custom_1"),
                Custom2 = Get("custom_2"),
                Custom3 = Get("custom_3"),
                Custom4 = Get("custom_4"),
                Custom5 = Get("custom_5")
            };
        }
    }
}
