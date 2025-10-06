using System.Globalization;

namespace OnlineRetailAPI.Models
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public int TotalAmount { get; set; }
        public List<OrderItemDto> Items { get; set; }

    }
}
