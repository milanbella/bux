namespace Bux.Controllers.Model.Api.LeaderBoardController
{
    public record LeaderBoardLine(string username, double buxAmount);
    public record GetLeaderBoardResponse(List<LeaderBoardLine> lines, int myPlace);
}
