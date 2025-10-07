namespace OnlineRetailAPI.Models.DTOs
{
    public class UpdateCartItemDto
    {
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
