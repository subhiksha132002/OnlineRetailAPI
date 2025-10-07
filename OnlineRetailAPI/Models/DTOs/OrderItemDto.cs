namespace OnlineRetailAPI.Models.DTOs
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal ProductPrice { get; set; }
        public string ImageUrl { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }

    }
}
