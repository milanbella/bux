using System;

namespace Bux.Dbo.Model
{
    [System.Serializable]
    public class AyeOfferWallCallback
    {
        public int Id { get; set; }
        public System.DateTime ReceivedAt { get; set; }
        public System.DateTime? RespondedAt { get; set; }
        public int? ResponseStatusCode { get; set; }
        public string? ResponseBody { get; set; }

        public string? TransactionId { get; set; }
        public decimal? PayoutUsd { get; set; }
        public decimal? CurrencyAmount { get; set; }
        public string? ExternalIdentifier { get; set; }
        public int? UserId { get; set; }
        public string? PlacementIdentifier { get; set; }
        public int? AdslotId { get; set; }
        public string? SubId { get; set; }
        public string? Ip { get; set; }
        public int? OfferId { get; set; }
        public string? OfferName { get; set; }
        public string? DeviceUuid { get; set; }
        public string? DeviceMake { get; set; }
        public string? DeviceModel { get; set; }
        public string? AdvertisingId { get; set; }
        public string? Sha1AndroidId { get; set; }
        public string? Sha1Imei { get; set; }
        public int IsChargeback { get; set; }
        public string? ChargebackReason { get; set; }
        public string? ChargebackDate { get; set; }
        public string? EventName { get; set; }
        public decimal? EventValue { get; set; }
        public string? TaskName { get; set; }
        public string? TaskUuid { get; set; }
        public string? CurrencyIdentifier { get; set; }
        public decimal? CurrencyConversionRate { get; set; }
        public string? ClickDate { get; set; }
        public long? CallbackTs { get; set; }
        public string? Custom1 { get; set; }
        public string? Custom2 { get; set; }
        public string? Custom3 { get; set; }
        public string? Custom4 { get; set; }
        public string? Custom5 { get; set; }
    }
}
