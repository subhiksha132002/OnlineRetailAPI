namespace OnlineRetailAPI.Models
{
    public class UpdateCartItemDto
    {
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
