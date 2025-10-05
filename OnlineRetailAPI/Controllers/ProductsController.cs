using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult GetAllProducts()
        {
            var allProducts = dbContext.Products.ToList();

            return Ok(allProducts);
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetProductById(int id)
        {
            var product = dbContext.Products.Find(id);

            if(product is null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        public IActionResult AddProduct(AddProductDto addProductDto)
        {
            var productEntity = new Product()
            {
                ProductName = addProductDto.ProductName,
                ProductDescription = addProductDto.ProductDescription,
                ProductPrice = addProductDto.ProductPrice,
                StockQuantity = addProductDto.StockQuantity,
                ImageUrl = addProductDto.ImageUrl
            };
            dbContext.Products.Add(productEntity);
            dbContext.SaveChanges();

            return CreatedAtAction(nameof(GetProductById),new { id = productEntity.ProductId },productEntity);
        }

        [HttpPut]
        [Route("{id:int}")]
        public IActionResult UpdateProduct(int id,UpdateProductDto updateProductDto)
        {
           var product = dbContext.Products.Find(id);

            if(product is null)
            {
                return NotFound();
            }

            product.ProductName = updateProductDto.ProductName;
            product.ProductDescription = updateProductDto.ProductDescription;
            product.ProductPrice = updateProductDto.ProductPrice;
            product.StockQuantity = updateProductDto.StockQuantity;
            product.ImageUrl = updateProductDto.ImageUrl;

            dbContext.SaveChanges();

            return Ok(product);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = dbContext.Products.Find(id);

            if(product is null)
            {
                return NotFound();

            }

            dbContext.Products.Remove(product);

            dbContext.SaveChanges();

            return NoContent();
        }
    }
}
