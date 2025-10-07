using Microsoft.EntityFrameworkCore;
using OnlineRetailAPI.Data;
using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Models.Entities;
using OnlineRetailAPI.Services.Interfaces;

namespace OnlineRetailAPI.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _dbContext.Products.ToListAsync();

        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _dbContext.Products.FindAsync(productId);

        }

        public async Task<Product> AddProductAsync(AddProductDto addProductDto)
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

            return productEntity;
        }

        public async Task<Product?> UpdateProductAsync(int productId, UpdateProductDto updateProductDto)
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

            return product;
        
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

            return true;
        }
    }
}
