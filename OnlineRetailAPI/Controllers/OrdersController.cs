using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRetailAPI.Data;
using OnlineRetailAPI.Models;
using OnlineRetailAPI.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace OnlineRetailAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public OrdersController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await dbContext.Orders.Include(o => o.Customer).Include(o => o.OrderItems).ThenInclude(oi => oi.Product).Select(o => new OrderDto
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

            return Ok(orders);
        }

        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var order = await dbContext.Orders.Include(o => o.Customer).Include(o => o.OrderItems).ThenInclude(oi => oi.Product).Where(o => o.OrderId == orderId).Select(o => new OrderDto
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

            if (order is null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpPost("PlaceOrder")]
        public async Task<IActionResult> PlaceOrder(AddOrderDto addOrderDto)
        {
            if (addOrderDto is null)
            {
                return BadRequest("Invalid order data.");
            }

            
            var customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == addOrderDto.CustomerId);
            if (customer is null)
            {
                return NotFound("Customer not found.");
            }

            
            var cart = await dbContext.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.CustomerId == addOrderDto.CustomerId);
            if (cart is null || !cart.CartItems.Any())
            {
                return NotFound("No items in cart.");
            }

            
            var order = new Order
            {
                CustomerId = addOrderDto.CustomerId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0
            };

            
            foreach (var cartItem in cart.CartItems)
            {
                var product = await dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == cartItem.ProductId);
                if (product is null)
                {
                    return NotFound($"Product with ID {cartItem.ProductId} not found.");
                }

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

            
            await dbContext.Orders.AddAsync(order);
            await dbContext.SaveChangesAsync();

            // Clear Cart Items After Order is Created
            dbContext.CartItems.RemoveRange(cart.CartItems);
            await dbContext.SaveChangesAsync();

            // Return Created Order
            var orderDto = new OrderDto
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerName = customer.CustomerName,
                Email = customer.Email,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.ProductName,
                    ProductPrice = oi.ProductPrice,
                    ImageUrl = oi.Product.ImageUrl,
                    Quantity = oi.Quantity,
                    SubTotal = oi.SubTotal
                }).ToList()
            };

            return CreatedAtAction(nameof(GetOrderById), new { orderId = order.OrderId }, orderDto);
        }

    }
}
