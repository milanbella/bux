namespace Bux.Controllers.Model.Api.ClickGameController
{
    public record ClickResponse(double buxAmount, int clicksCount);

    public record GameState(int clicksCount);
}
