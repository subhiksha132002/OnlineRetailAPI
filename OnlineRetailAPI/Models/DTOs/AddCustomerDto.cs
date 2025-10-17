namespace OnlineRetailAPI.Models.DTOs
{
    public class AddCustomerDto
    {
        public required string CustomerName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public  string? Address { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
