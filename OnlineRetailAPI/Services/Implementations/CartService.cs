using Microsoft.EntityFrameworkCore;
using OnlineRetailAPI.Data;
using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Models.Entities;
using OnlineRetailAPI.Services.Interfaces;

namespace OnlineRetailAPI.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _dbContext;

        public CartService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<CartDto>> GetAllCartsAsync()
        {
            return await _dbContext.Carts
                .Include(c => c.Customer)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .Select(c => new CartDto
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
        }

        public async Task<CartDto?> GetCartByCustomerIdAsync(int customerId)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.Customer)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null) return null;

            return new CartDto
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
        }

        public async Task AddItemToCartAsync(AddCartItemDto addCartDto)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == addCartDto.CustomerId);

            if (cart == null)
            {
                cart = new Cart { CustomerId = addCartDto.CustomerId };
                await _dbContext.Carts.AddAsync(cart);
                await _dbContext.SaveChangesAsync();
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == addCartDto.ProductId);

            if (existingItem != null)
                existingItem.Quantity += addCartDto.Quantity;
            else
                await _dbContext.CartItems.AddAsync(new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = addCartDto.ProductId,
                    Quantity = addCartDto.Quantity
                });

            await _dbContext.SaveChangesAsync();
           
        }

        public async Task<bool> UpdateItemQuantityAsync(UpdateCartItemDto updateCartItemDto)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == updateCartItemDto.CustomerId);

            if (cart == null) return false;

            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == updateCartItemDto.ProductId);
            if (item == null) return false;

            item.Quantity = updateCartItemDto.Quantity;
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteCartItemAsync(int customerId, int cartItemId)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null) return false;

            var item = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId);
            if (item == null) return false;

            _dbContext.CartItems.Remove(item);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ClearCartAsync(int customerId)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null) return false;

            if (!cart.CartItems.Any()) return true;

            _dbContext.CartItems.RemoveRange(cart.CartItems);
            await _dbContext.SaveChangesAsync();

            return true;
        }


    }
}
