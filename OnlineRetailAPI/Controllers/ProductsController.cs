using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Services.Interfaces;
using System.Security.AccessControl;


namespace OnlineRetailAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService _productService)
        {
            this._productService = _productService;
            
        }

        [HttpGet]
        [Authorize] //Accessible to all authenticated users
        public async Task<IActionResult> GetAllProducts()
        {

            var allProducts = await _productService.GetAllProductsAsync();

            return Ok(allProducts);
        }


        [HttpGet("{productId:int}")]
        [Authorize] //Accessible to all authenticated users
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
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AddProduct(AddProductDto addProductDto)
        {
            var product = await _productService.AddProductAsync(addProductDto);

            return CreatedAtAction(nameof(GetProductById),new { productId = product.ProductId },product);
        }

        [HttpPut("{productId:int}/UpdateProduct")]
        [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "AdminOnly")]
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
