using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Distributed;
using OnlineRetailAPI.Data;
using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Models.Entities;
using OnlineRetailAPI.Services.Interfaces;
using System.Text.Json;

namespace OnlineRetailAPI.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDistributedCache _cache;

        private const string AllCartsCacheKey = "carts";
        private static string CartOfCustomerCacheKey(int customerId) => $"cart:customer:{customerId}";


        public CartService(ApplicationDbContext dbContext, IDistributedCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        private async Task<T?> GetFromCacheAsync<T>(string key)
        {
            var cachedData = await _cache.GetStringAsync(key);
            return string.IsNullOrEmpty(cachedData) ? default : JsonSerializer.Deserialize<T>(cachedData);
        }


        private async Task SetToCacheAsync<T>(string key, T obj, TimeSpan expiration)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            string jsonData = JsonSerializer.Serialize(obj);
            await _cache.SetStringAsync(key, jsonData, options);
        }

        private async Task RemoveFromCacheAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }


        public async Task<IEnumerable<CartDto>> GetAllCartsAsync()
        {

            //var cached = await GetFromCacheAsync<IEnumerable<CartDto>>(AllCartsCacheKey);
            //if (cached != null)
                //return cached;

            var cartsDto =  await _dbContext.Carts
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

            //await SetToCacheAsync(AllCartsCacheKey, cartsDto, TimeSpan.FromMinutes(5));

            return cartsDto;
        }

        public async Task<CartDto?> GetCartByCustomerIdAsync(int customerId)
        {
            //var cacheKey = CartOfCustomerCacheKey(customerId);
            //var cached = await GetFromCacheAsync<CartDto>(cacheKey);
            //if (cached != null)
                //return cached;

            var cart = await _dbContext.Carts
                .Include(c => c.Customer)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null) return null;

            var cartDto =  new CartDto
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

            //await SetToCacheAsync(cacheKey, cartDto, TimeSpan.FromMinutes(5));
            return cartDto;
        }

        public async Task AddItemToCartAsync(AddCartItemDto addCartDto)
        {
            // Invalidate cache for all carts and this customer's cart
            //await RemoveFromCacheAsync(AllCartsCacheKey);
            //await RemoveFromCacheAsync(CartOfCustomerCacheKey(addCartDto.CustomerId));

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
            //await RemoveFromCacheAsync(AllCartsCacheKey);
            //await RemoveFromCacheAsync(CartOfCustomerCacheKey(updateCartItemDto.CustomerId));


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
            //await RemoveFromCacheAsync(AllCartsCacheKey);
            //await RemoveFromCacheAsync(CartOfCustomerCacheKey(customerId));


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
            //await RemoveFromCacheAsync(AllCartsCacheKey);
            //await RemoveFromCacheAsync(CartOfCustomerCacheKey(customerId));

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
