namespace Bux.Controllers.Model.Api1.EarnersController
{
    public record Earner(int userId, string username, double amount);
    public record TopEarnersResponse(List<Earner> earners);

    public record LastRedeemerDto(int UserId, string UserName, double AmountRedeemed, DateTime RedeemedAt);
    public record LastRedeemersResponse(List<LastRedeemerDto> Items);
}
