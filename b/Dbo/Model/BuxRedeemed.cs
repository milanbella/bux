namespace Bux.Dbo.Model
{
    public class BuxRedeemed
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public double Amount { get; set; }
        public double Amount1 { get; set; }
        public System.DateTime RedeemedAt { get; set; }
    }
}
