using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Services.Interfaces;


namespace OnlineRetailAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound(new { message = "Order not found." });
            return Ok(order);
        }

        [HttpGet("Customer/{customerId}")]
        [Authorize]
        public async Task<IActionResult> GetOrdersByCustomerId(int customerId)
        {
            var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
            return Ok(orders);
        }

        [HttpPost("PlaceOrder")]
        [Authorize]
        public async Task<IActionResult> PlaceOrder(AddOrderDto addOrderDto)
        {
            if (addOrderDto == null)
                return BadRequest(new { message = "Invalid order data." });

            var order = await _orderService.PlaceOrderAsync(addOrderDto);
            if (order == null)
                return NotFound(new { message = "Customer not found or cart is empty." });

            return CreatedAtAction(nameof(GetOrderById), new { orderId = order.OrderId }, order);
        }

    }
}
