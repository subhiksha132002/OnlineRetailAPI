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
        public IActionResult GetAllCarts()
        {
            var carts = dbContext.Carts.Include(c => c.Customer).Include(c => c.CartItems).ThenInclude(ci => ci.Product).Select(c => new CartDto
            {
                CartId = c.CartId,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer.CustomerName,
                Email = c.Customer.Email,
                Items = c.CartItems.Select(ci => new CartItemDto
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.ProductName,
                    ProductPrice = ci.Product.ProductPrice,
                    ImageUrl = ci.Product.ImageUrl,
                    Quantity = ci.Quantity
                }).ToList()
            }).ToList();

            return Ok(carts);
        }

        [HttpGet]
        [Route("{customerId:int}")]
        public IActionResult GetCartByCustomerId(int customerId)
        {
            var cart = dbContext.Carts.Include(c => c.Customer).Include(c => c.CartItems).ThenInclude(ci => ci.Product).FirstOrDefault(c => c.CustomerId == customerId);

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
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.ProductName,
                    Quantity = ci.Quantity

                }).ToList()
            };
            return Ok(cartDto);
        }

        [HttpPost]
        public IActionResult AddItemToCart(AddCartItemDto addCartItemDto)
        {
            var cart = dbContext.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.CustomerId == addCartItemDto.CustomerId);

            if (cart is null)
            {
                cart = new Cart
                {
                    CustomerId = addCartItemDto.CustomerId
                };

                dbContext.Carts.Add(cart);
                dbContext.SaveChanges();
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == addCartItemDto.ProductId);

            if (existingItem != null)
            {
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

                dbContext.CartItems.Add(cartItem);
               
            }
            dbContext.SaveChanges();

            return Ok("Item added to Cart Successfully");
        }

        //To update quantity in cartitem
        [HttpPut]
        public IActionResult UpdateItemInCart(UpdateCartItemDto updateCartItemDto)
        {
            var cart = dbContext.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.CustomerId == updateCartItemDto.CustomerId);

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
            dbContext.SaveChanges();

            return Ok("Quantity updated successfully.");
        }

        [HttpDelete]
        [Route("{customerId:int}")]
        public IActionResult ClearCart(int customerId)
        {
            var cart = dbContext.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.CustomerId == customerId);

            if (cart is null)
            {
                return NotFound("Cart not found for this customer.");

            }

            if (!cart.CartItems.Any())
            {
                return Ok("Cart is already empty.");
            }

            dbContext.CartItems.RemoveRange(cart.CartItems);
            dbContext.SaveChanges();

            return Ok("Cart cleared successfully.");
        }

    }
}
