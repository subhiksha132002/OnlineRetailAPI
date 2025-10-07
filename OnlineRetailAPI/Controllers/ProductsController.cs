using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRetailAPI.Data;
using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Models.Entities;
using OnlineRetailAPI.Services.Interfaces;
using System.ComponentModel;

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
        public async Task<IActionResult> GetAllProducts()
        {
            var allProducts = await _productService.GetAllProductsAsync();

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
            var success = await _productService.DeleteProductAsync(productId);

            if (!success)
            {
                return NotFound();

            }

            return NoContent();
        }
    }
}
