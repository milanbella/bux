using System;

namespace Bux.Dbo.Model
{
    [Serializable]
    public class CpxOfferWallCallback
    {
        public int Id { get; set; }
        public DateTime ReceivedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public int? ResponseStatusCode { get; set; }
        public string? ResponseBody { get; set; }

        public int? Status { get; set; }
        public string? TransactionId { get; set; }
        public string? UserId { get; set; }
        public string? SubId1 { get; set; }
        public string? SubId2 { get; set; }
        public decimal? AmountLocal { get; set; }
        public decimal? AmountUsd { get; set; }
        public string? OfferId { get; set; }
        public string? Hash { get; set; }
        public string? IpClick { get; set; }
        public string? Type { get; set; }
    }
}
