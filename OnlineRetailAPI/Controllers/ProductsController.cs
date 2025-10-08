using Microsoft.AspNetCore.Mvc;
using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Models.Entities;
using OnlineRetailAPI.Services.Caching;
using OnlineRetailAPI.Services.Interfaces;


namespace OnlineRetailAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IRedisCacheService _cache;

        public ProductsController(IProductService _productService,IRedisCacheService cache)
        {
            this._productService = _productService;
            this._cache = cache;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var allProducts = _cache.GetData<IEnumerable<Product>>("products");
            Console.WriteLine(allProducts);

            if(allProducts is not null)
            {
                return Ok(allProducts);
            }

            allProducts = await _productService.GetAllProductsAsync();

            _cache.SetData("products", allProducts);
            return Ok(allProducts);
        }

        [HttpGet("{productId:int}")]
        public async Task<IActionResult> GetProductById(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);

            if(product is null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost("CreateProduct")]
        public async Task<IActionResult> AddProduct(AddProductDto addProductDto)
        {
            var product = await _productService.AddProductAsync(addProductDto);
            return CreatedAtAction(nameof(GetProductById),new { productId = product.ProductId },product);
        }

        [HttpPut("{productId:int}/UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(int productId, UpdateProductDto updateProductDto)
        {
           var product = await _productService.UpdateProductAsync(productId,updateProductDto);

            if(product is null)
            {
                return NotFound();
            }   

            return Ok(product);
        }

        [HttpDelete("{productId:int}/DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var deleted = await _productService.DeleteProductAsync(productId);

            if (!deleted)
            {
                return NotFound();

            }

            return NoContent();
        }
    }
}
