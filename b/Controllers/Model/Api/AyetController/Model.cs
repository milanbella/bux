namespace bux.Controllers.Model.Api.AyetController
{
    public record GetOfferWallAddSlotLinkResponse(string link);

	public record ConversionsListItem(System.DateTime? ReceivedAt, string? TransactionId, decimal? PayoutUsd, decimal? CurrencyAmount, string? CurrencyIdentifier, decimal? CurrencyConversionRate,  int? UserId, string? ExternalIdentifier, string? PlacementIdentifier, int? AdslotId, int? OfferId, string? OfferName, int IsChargeback, string? ChargebackReason, string? ChargebackDate);
    public record ConversionsListResponse(List<ConversionsListItem> conversions);
}
