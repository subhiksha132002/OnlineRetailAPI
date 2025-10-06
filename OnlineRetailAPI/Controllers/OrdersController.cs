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
        public IActionResult AddOrder(AddOrderDto addOrderDto)
        {

        }
    }
}
