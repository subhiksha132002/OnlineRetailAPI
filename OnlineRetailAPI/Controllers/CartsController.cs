using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRetailAPI.Data;
using OnlineRetailAPI.Models;
using OnlineRetailAPI.Models.Entities;

namespace OnlineRetailAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public CartsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCarts()
        {
            var carts = await dbContext.Carts.Include(c => c.Customer).Include(c => c.CartItems).ThenInclude(ci => ci.Product).Select(c => new CartDto
            {
                CartId = c.CartId,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer.CustomerName,
                Email = c.Customer.Email,
                Items = c.CartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.CartItemId,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.ProductName,
                    ProductPrice = ci.Product.ProductPrice,
                    ImageUrl = ci.Product.ImageUrl,
                    Quantity = ci.Quantity
                }).ToList()
            }).ToListAsync();

            return Ok(carts);
        }

        [HttpGet("{customerId:int}")]
        public async Task<IActionResult> GetCartByCustomerId(int customerId)
        {
            var cart = await dbContext.Carts.Include(c => c.Customer).Include(c => c.CartItems).ThenInclude(ci => ci.Product).FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart is null)
            {
                return NotFound("Cart not found for the customer.");
            }

            var cartDto = new CartDto
            {
                CartId = cart.CartId,
                CustomerId = cart.CustomerId,
                CustomerName = cart.Customer.CustomerName,
                Email = cart.Customer.Email,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.CartItemId,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.ProductName,
                    ProductPrice = ci.Product.ProductPrice,
                    ImageUrl = ci.Product.ImageUrl,
                    Quantity = ci.Quantity

                }).ToList()
            };
            return Ok(cartDto);
        }

        [HttpPost("AddItemToCart")]
        public async Task<IActionResult> AddItemToCart(AddCartItemDto addCartItemDto)
        {
            var cart = await dbContext.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.CustomerId == addCartItemDto.CustomerId);

            if (cart is null)
            {
                cart = new Cart
                {
                    CustomerId = addCartItemDto.CustomerId
                };

                await dbContext.Carts.AddAsync(cart);
                await dbContext.SaveChangesAsync();
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == addCartItemDto.ProductId);

            if (existingItem != null)
            {
                //Update Quantity
                existingItem.Quantity += addCartItemDto.Quantity;
            }

            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = addCartItemDto.ProductId,
                    Quantity = addCartItemDto.Quantity
                };

                await dbContext.CartItems.AddAsync(cartItem);

            }
            await dbContext.SaveChangesAsync();

            return Ok(new { message = "Item added to Cart Successfully" });
        }

        //To update quantity in cartitem
        [HttpPut("UpdateQuantity")]
        public async Task<IActionResult> UpdateItemInCart(UpdateCartItemDto updateCartItemDto)
        {
            var cart = await dbContext.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.CustomerId == updateCartItemDto.CustomerId);

            if (cart is null)
            {
                return NotFound("Cart not found for the Customer.");

            }

            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == updateCartItemDto.ProductId);

            if (item is null)
            {
                return NotFound("Product not found in the cart");
            }

            item.Quantity = updateCartItemDto.Quantity;
            await dbContext.SaveChangesAsync();

            return Ok("Quantity updated successfully.");
        }

        [HttpDelete("{customerId:int}/Items/{cartItemId:int}")]
        public async Task<IActionResult> DeleteCartItem(int customerId,int cartItemId)
        {
            var cart = await dbContext.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart is null)
            {
                return NotFound("Cart not found for this customer.");

            }

            var cartItem = await dbContext.CartItems.FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.CartId == cart.CartId);

            if (cartItem == null)
            {
                return NotFound(new { message = "Cart item not found for this customer." });
            }

            dbContext.CartItems.Remove(cartItem);
            await dbContext.SaveChangesAsync();

            return Ok(new { message = "Cart item deleted successfully." });

        }


        [HttpDelete("{customerId:int}/ClearCart")]
        public async Task<IActionResult> ClearCart(int customerId)
        {
            var cart = await dbContext.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart is null)
            {
                return NotFound("Cart not found for this customer.");

            }

            if (!cart.CartItems.Any())
            {
                return Ok("Cart is already empty.");
            }

            dbContext.CartItems.RemoveRange(cart.CartItems);
            await dbContext.SaveChangesAsync();

            return Ok("Cart cleared successfully.");
        }

    }
}
