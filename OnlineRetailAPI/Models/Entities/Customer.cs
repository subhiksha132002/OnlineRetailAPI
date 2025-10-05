namespace OnlineRetailAPI.Models.Entities
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public required string CustomerName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Address { get; set; }
        public string? PhoneNumber { get; set; }

    }
}
