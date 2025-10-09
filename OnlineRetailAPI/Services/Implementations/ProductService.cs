using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OnlineRetailAPI.Data;
using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Models.Entities;
using OnlineRetailAPI.Services.Interfaces;
using System.Text.Json;

namespace OnlineRetailAPI.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDistributedCache _cache;

        private const string AllProductsCacheKey = "products";
        private static string ProductCacheKey(int productId) => $"product:{productId}";

        public ProductService(ApplicationDbContext dbContext, IDistributedCache cache)
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



        public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync()
        {

            var cachedProducts = await GetFromCacheAsync<IEnumerable<ProductResponseDto>>(AllProductsCacheKey);

            if (cachedProducts != null)
                return cachedProducts;

            var products =  await _dbContext.Products.ToListAsync();

            var productsDto = products.Select(p => new ProductResponseDto
            {

                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductPrice = p.ProductPrice,
                StockQuantity = p.StockQuantity,
                ProductDescription = p.ProductDescription,
                ImageUrl = p.ImageUrl
            }).ToList();

            await SetToCacheAsync(AllProductsCacheKey, productsDto, TimeSpan.FromMinutes(5));

            return productsDto;

        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(int productId)
        {
            var cacheKey = ProductCacheKey(productId);
            var cachedProduct = await GetFromCacheAsync<ProductResponseDto>(cacheKey);

            if (cachedProduct != null)
                return cachedProduct;

            var product = await _dbContext.Products.FindAsync(productId);

            if (product == null) return null;
            
            var productDto =  new ProductResponseDto
             {
                 ProductId = product.ProductId,
                 ProductName = product.ProductName,
                 ProductPrice = product.ProductPrice,
                 StockQuantity = product.StockQuantity,
                 ProductDescription = product.ProductDescription,
                 ImageUrl = product.ImageUrl
                };

                await SetToCacheAsync(cacheKey, productDto, TimeSpan.FromMinutes(5));


            return productDto;

        }

        public async Task<ProductResponseDto> AddProductAsync(AddProductDto addProductDto)
        {
            var productEntity = new Product()
            {
                ProductName = addProductDto.ProductName,
                ProductDescription = addProductDto.ProductDescription,
                ProductPrice = addProductDto.ProductPrice,
                StockQuantity = addProductDto.StockQuantity,
                ImageUrl = addProductDto.ImageUrl
            };
            await _dbContext.Products.AddAsync(productEntity);
            await _dbContext.SaveChangesAsync();

            await RemoveFromCacheAsync(AllProductsCacheKey);

            var productDto = new ProductResponseDto
            {
                ProductName = productEntity.ProductName,
                ProductDescription = productEntity.ProductDescription,
                ProductPrice = productEntity.ProductPrice,
                StockQuantity = productEntity.StockQuantity,
                ImageUrl = productEntity.ImageUrl
            };

            return productDto;
        }

        public async Task<ProductResponseDto?> UpdateProductAsync(int productId, UpdateProductDto updateProductDto)
        {
            var product = await _dbContext.Products.FindAsync(productId);

            if (product is null)
            {
                return null;
            }

            product.ProductName = updateProductDto.ProductName;
            product.ProductDescription = updateProductDto.ProductDescription;
            product.ProductPrice = updateProductDto.ProductPrice;
            product.StockQuantity = updateProductDto.StockQuantity;
            product.ImageUrl = updateProductDto.ImageUrl;

            await _dbContext.SaveChangesAsync();

            //Invalidate Caches
            await RemoveFromCacheAsync(AllProductsCacheKey);
            await RemoveFromCacheAsync(ProductCacheKey(productId));

            var productDto = new ProductResponseDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                ProductPrice = product.ProductPrice,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl
            };

            return productDto;
        
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _dbContext.Products.FindAsync(productId);

            if (product is null)
            {
                return false;

            }

            _dbContext.Products.Remove(product);

            await _dbContext.SaveChangesAsync();

            await RemoveFromCacheAsync(AllProductsCacheKey);
            await RemoveFromCacheAsync(ProductCacheKey(productId));

            return true;
        }
    }
}
