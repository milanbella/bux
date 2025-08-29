namespace Bux.Controllers.Model.Api.UserController
{
    public record GetUserResponse(string username, string userEmail, string firstName, string lastName);
    public record GetTotalBuxEarnedResponse(double totalBux);

    public record AvatarUploadResponse(string url64, string url128, string url256);

    public record GetAvatarsResponse(string url64, string url128, string url256);
    public record GetAvatar64Response(string url64);

    public record GetReferralsCountResponse(int referralsCount);
}
