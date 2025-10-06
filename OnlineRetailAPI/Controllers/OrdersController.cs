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
        public IActionResult GetAllOrders()
        {
            var orders = dbContext.Orders.Include(o => o.Customer).Include(o => o.OrderItems).ThenInclude(oi => oi.Product).Select(o => new OrderDto
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

            }).ToList();

            return Ok(orders);
        }

        [HttpGet]
        [Route("{orderId:int}")]
        public IActionResult GetOrderById(int orderId)
        {
            var order = dbContext.Orders.Include(o => o.Customer).Include(o => o.OrderItems).ThenInclude(oi => oi.Product).Where(o => o.OrderId == orderId).Select(o => new OrderDto
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
            }).FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpPost]
        public IActionResult PlaceOrder(AddOrderDto addOrderDto)
        {
            if (addOrderDto is null)
            {
                return BadRequest("Invalid order data.");
            }

            // Step 1: Check if the customer exists
            var customer = dbContext.Customers.FirstOrDefault(c => c.CustomerId == addOrderDto.CustomerId);
            if (customer is null)
            {
                return NotFound("Customer not found.");
            }

            // Step 2: Get Cart Items for the customer
            var cart = dbContext.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.CustomerId == addOrderDto.CustomerId);
            if (cart is null || !cart.CartItems.Any())
            {
                return NotFound("No items in cart.");
            }

            // Step 3: Create an Order
            var order = new Order
            {
                CustomerId = addOrderDto.CustomerId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0
            };

            // Step 4: Process Cart Items into Order Items
            foreach (var cartItem in cart.CartItems)
            {
                var product = dbContext.Products.FirstOrDefault(p => p.ProductId == cartItem.ProductId);
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

            
            dbContext.Orders.Add(order);
            dbContext.SaveChanges();

            // Clear Cart Items After Order is Created
            dbContext.CartItems.RemoveRange(cart.CartItems);
            dbContext.SaveChanges();

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
