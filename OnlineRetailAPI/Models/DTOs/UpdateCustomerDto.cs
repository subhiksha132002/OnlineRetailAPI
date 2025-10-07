namespace OnlineRetailAPI.Models.DTOs
{
    public class UpdateCustomerDto
    {
        public required string CustomerName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Address { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
