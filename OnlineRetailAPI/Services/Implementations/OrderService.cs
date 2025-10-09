using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OnlineRetailAPI.Data;
using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Models.Entities;
using OnlineRetailAPI.Services.Interfaces;
using System.Text.Json;

namespace OnlineRetailAPI.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDistributedCache _cache;
        private const string AllOrdersCacheKey = "orders";
        private static string OrderCacheKey(int orderId) => $"order:{orderId}";
        private static string OrdersOfCustomerCacheKey(int customerId) => $"orders:customer:{customerId}";

        public OrderService(ApplicationDbContext dbContext, IDistributedCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        private async Task<T?> GetFromCacheAsync<T>(string key)
        {
            var cachedData = await _cache.GetStringAsync(key);
            return string.IsNullOrEmpty(cachedData) ? default : JsonSerializer.Deserialize<T>(cachedData);
        }


        private async Task SetToCacheAsync<T>(string key, T obj, TimeSpan expiration)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            string jsonData = JsonSerializer.Serialize(obj);
            await _cache.SetStringAsync(key, jsonData, options);
        }

        private async Task RemoveFromCacheAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }


        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var cached = await GetFromCacheAsync<IEnumerable<OrderDto>>(AllOrdersCacheKey);
            if (cached != null)
                return cached;

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

            await SetToCacheAsync(AllOrdersCacheKey, orders, TimeSpan.FromMinutes(1));

            return orders;
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
        {
            var cacheKey = OrderCacheKey(orderId);
            var cached = await GetFromCacheAsync<OrderDto>(cacheKey);

            if (cached != null)
                return cached;

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


            await SetToCacheAsync(cacheKey, order, TimeSpan.FromMinutes(1));
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

            await RemoveFromCacheAsync(AllOrdersCacheKey);
            await RemoveFromCacheAsync(OrderCacheKey(order.OrderId));
            await RemoveFromCacheAsync(OrdersOfCustomerCacheKey(addOrderDto.CustomerId));

            var placedOrder = await GetOrderByIdAsync(order.OrderId);

            return placedOrder; 
        }


}
}
