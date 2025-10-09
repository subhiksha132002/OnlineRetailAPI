using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Models.Entities;

namespace OnlineRetailAPI.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync();
        Task<ProductResponseDto?> GetProductByIdAsync(int id);
        Task<ProductResponseDto> AddProductAsync(AddProductDto addProductDto);
        Task<ProductResponseDto?> UpdateProductAsync(int id, UpdateProductDto upadateProductDto);
        Task<bool> DeleteProductAsync(int id);
    }
}
