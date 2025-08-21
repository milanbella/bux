namespace Bux.Controllers.Model.Api.GuessGameController
{
    public record ClickRequest(int number);
    public record ClickResponse(double buxAmount, bool isMatch);

    public record GameState(int min, int max);
}
