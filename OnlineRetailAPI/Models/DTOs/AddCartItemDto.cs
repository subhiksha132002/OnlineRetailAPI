namespace OnlineRetailAPI.Models.DTOs
{
    public class AddCartItemDto
    {
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
