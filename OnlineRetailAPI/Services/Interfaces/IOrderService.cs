using OnlineRetailAPI.Models.DTOs;

namespace OnlineRetailAPI.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto?> GetOrderByIdAsync(int orderId);
        Task<OrderDto?> PlaceOrderAsync(AddOrderDto addOrderDto);
    }
}
