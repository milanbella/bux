namespace Bux.Dbo.Model
{
    public class ClickGame
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public int Clicks { get; set; }
    }
}
