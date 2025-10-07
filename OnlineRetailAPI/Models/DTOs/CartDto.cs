namespace OnlineRetailAPI.Models.DTOs
{
    public class CartDto
    {
        public int CartId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public List<CartItemDto> Items { get; set; } = new();
    }
}
