﻿namespace OnlineRetailAPI.Models.Entities
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public decimal ProductPrice { get; set; }

        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
    }
}
