namespace Bux.Controllers.Model.Api.LeaderBoardController
{
    public record LeaderBoardLine(string username, double buxAmount, string avatarUrl64);
    public record GetLeaderBoardResponse(List<LeaderBoardLine> lines, int myPlace); // returned lines are linnes from BuxEarned table ordered by bux amount, myPlace is the place of cuurently logged in user
}
