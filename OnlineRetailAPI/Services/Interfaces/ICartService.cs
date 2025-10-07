using OnlineRetailAPI.Models.DTOs;

namespace OnlineRetailAPI.Services.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<CartDto>> GetAllCartsAsync();
        Task<CartDto?> GetCartByCustomerIdAsync(int customerId);
        Task AddItemToCartAsync(AddCartItemDto addCartItemDto);
        Task<bool> UpdateItemQuantityAsync(UpdateCartItemDto updateCartItemDto);
        Task<bool> DeleteCartItemAsync(int customerId, int cartItemId);
        Task<bool> ClearCartAsync(int customerId);
    }
}
