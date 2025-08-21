namespace Bux.Dbo.Model
{
    public class GuessGame
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public int number { get; set; }
    }
}
