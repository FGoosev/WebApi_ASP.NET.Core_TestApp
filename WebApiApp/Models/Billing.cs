namespace WebApiApp.Models
{
    public class Billing
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Total { get; set; }
        public DateTime DateOrder { get; set; }
        public User User { get; set; }
    }
}
