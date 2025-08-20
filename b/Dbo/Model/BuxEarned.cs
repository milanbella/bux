namespace Bux.Dbo.Model
{
    public class BuxEarned
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public double Amount { get; set; }
    }
}
