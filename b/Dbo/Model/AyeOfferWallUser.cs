#pragma warning disable 8632

namespace Bux.Dbo.Model
{
    [System.Serializable]
    public class AyeOfferWallUser
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string? AyeUserId { get; set; }
    }
}
