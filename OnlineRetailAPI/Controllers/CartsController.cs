using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Services.Interfaces;

namespace OnlineRetailAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCarts()
        {
            var carts = await _cartService.GetAllCartsAsync();
            return Ok(carts);
        }

        [HttpGet("{customerId:int}")]
        public async Task<IActionResult> GetCartByCustomerId(int customerId)
        {
            var cart = await _cartService.GetCartByCustomerIdAsync(customerId);
            if (cart == null)
                return NotFound(new { message = "Cart not found for the customer." });

            return Ok(cart);
        }

        [HttpPost("AddItemToCart")]
        public async Task<IActionResult> AddItemToCart(AddCartItemDto addCartItemDto)
        {
            await _cartService.AddItemToCartAsync(addCartItemDto);
            return Ok(new { message = "Item added to cart successfully." });
        }

        //To update quantity in cartitem
        [HttpPut("UpdateQuantity")]
        public async Task<IActionResult> UpdateItemInCart(UpdateCartItemDto updateCartItemDto)
        {
            var success = await _cartService.UpdateItemQuantityAsync(updateCartItemDto);

            if (!success)
                return NotFound(new { message = "Cart or product not found." });
            return Ok(new { message = "Quantity updated successfully." });
        }

        [HttpDelete("{customerId:int}/Items/{cartItemId:int}")]
        public async Task<IActionResult> DeleteCartItem(int customerId,int cartItemId)
        {
            var success = await _cartService.DeleteCartItemAsync(customerId, cartItemId);

            if (!success)
                return NotFound(new { message = "Cart or cart item not found." });
            return Ok(new { message = "Cart item deleted successfully." });

        }


        [HttpDelete("{customerId:int}/ClearCart")]
        public async Task<IActionResult> ClearCart(int customerId)
        {
            var success = await _cartService.ClearCartAsync(customerId);
            if (!success)
                return NotFound(new { message = "Cart not found." });
            return Ok(new { message = "Cart cleared successfully." });
        }

    }
}
