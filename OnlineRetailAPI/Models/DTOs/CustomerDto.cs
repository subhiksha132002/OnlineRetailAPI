namespace OnlineRetailAPI.Models.DTOs
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public required string CustomerName { get; set; }
        public required string Email { get; set; }
        public required string Address { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
