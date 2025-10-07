using Microsoft.EntityFrameworkCore;
using OnlineRetailAPI.Data;
using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Models.Entities;
using OnlineRetailAPI.Services.Interfaces;

namespace OnlineRetailAPI.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Select(o => new OrderDto
                {
                    OrderId = o.OrderId,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.CustomerName,
                    Email = o.Customer.Email,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.ProductName,
                        ProductPrice = oi.Product.ProductPrice,
                        ImageUrl = oi.Product.ImageUrl,
                        Quantity = oi.Quantity,
                        SubTotal = oi.SubTotal
                    }).ToList()
                }).ToListAsync();

            return orders;
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderId == orderId)
                .Select(o => new OrderDto
                {
                    OrderId = o.OrderId,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.CustomerName,
                    Email = o.Customer.Email,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.ProductName,
                        ProductPrice = oi.Product.ProductPrice,
                        ImageUrl = oi.Product.ImageUrl,
                        Quantity = oi.Quantity,
                        SubTotal = oi.SubTotal
                    }).ToList()
                }).FirstOrDefaultAsync();

            return order;
        }

        public async Task<OrderDto?> PlaceOrderAsync(AddOrderDto addOrderDto)
        {
            var customer = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == addOrderDto.CustomerId);

            if (customer is null)
                return null;

            var cart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == addOrderDto.CustomerId);

            if (cart is null || !cart.CartItems.Any())
                return null;

            var order = new Order
            {
                CustomerId = addOrderDto.CustomerId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0
            };

            foreach (var cartItem in cart.CartItems)
            {
                var product = cartItem.Product;

                var orderItem = new OrderItem
                {
                    ProductId = product.ProductId,
                    ProductPrice = product.ProductPrice,
                    Quantity = cartItem.Quantity,
                    SubTotal = product.ProductPrice * cartItem.Quantity
                };

                order.TotalAmount += orderItem.SubTotal;
                order.OrderItems.Add(orderItem);
            }

            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            _dbContext.CartItems.RemoveRange(cart.CartItems);
            await _dbContext.SaveChangesAsync();

            var placedOrder = await GetOrderByIdAsync(order.OrderId);

            return placedOrder; 
        }


}
}
