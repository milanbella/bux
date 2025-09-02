namespace Bux.Controllers.Model.Api1.EarnersController
{
    public record Earner(int userId, string username, double amount);
    public record TopEarnersResponse(List<Earner> earners);
}
