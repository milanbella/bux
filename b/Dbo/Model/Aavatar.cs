#pragma warning disable 8632
namespace Bux.Dbo.Model
{
    [System.Serializable]
    public class Avatar
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string? path64 { get; set; }
        public string? file64 { get; set; }
        public string? path128 { get; set; }
        public string? file128 { get; set; }
        public string? path256 { get; set; }
        public string? file256 { get; set; }
    }
}
