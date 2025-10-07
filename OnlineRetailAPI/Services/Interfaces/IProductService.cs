using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Models.Entities;

namespace OnlineRetailAPI.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> AddProductAsync(AddProductDto addProductDto);
        Task<Product?> UpdateProductAsync(int id, UpdateProductDto upadateProductDto);
        Task<bool> DeleteProductAsync(int id);
    }
}
