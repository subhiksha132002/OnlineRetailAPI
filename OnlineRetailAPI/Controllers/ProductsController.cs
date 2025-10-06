using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRetailAPI.Data;
using OnlineRetailAPI.Models;
using OnlineRetailAPI.Models.Entities;
using System.ComponentModel;

namespace OnlineRetailAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public ProductsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var allProducts = await dbContext.Products.ToListAsync();

            return Ok(allProducts);
        }

        [HttpGet("{productId:int}")]
        public async Task<IActionResult> GetProductById(int productId)
        {
            var product = await dbContext.Products.FindAsync(productId);

            if(product is null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost("CreateProduct")]
        public async Task<IActionResult> AddProduct(AddProductDto addProductDto)
        {
            var productEntity = new Product()
            {
                ProductName = addProductDto.ProductName,
                ProductDescription = addProductDto.ProductDescription,
                ProductPrice = addProductDto.ProductPrice,
                StockQuantity = addProductDto.StockQuantity,
                ImageUrl = addProductDto.ImageUrl
            };
            await dbContext.Products.AddAsync(productEntity);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById),new { productId = productEntity.ProductId },productEntity);
        }

        [HttpPut("{productId:int}/UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(int productId, UpdateProductDto updateProductDto)
        {
           var product = await dbContext.Products.FindAsync(productId);

            if(product is null)
            {
                return NotFound();
            }

            product.ProductName = updateProductDto.ProductName;
            product.ProductDescription = updateProductDto.ProductDescription;
            product.ProductPrice = updateProductDto.ProductPrice;
            product.StockQuantity = updateProductDto.StockQuantity;
            product.ImageUrl = updateProductDto.ImageUrl;

            await dbContext.SaveChangesAsync();

            return Ok(product);
        }

        [HttpDelete("{productId:int}/DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var product = await dbContext.Products.FindAsync(productId);

            if (product is null)
            {
                return NotFound();

            }

            dbContext.Products.Remove(product);

            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
