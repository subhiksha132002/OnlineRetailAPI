using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Models.Entities;

namespace OnlineRetailAPI.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto?> GetCustomerByIdAsync(int id);
        Task<CustomerDto?> GetCustomerByEmailAsync(string email);
        Task<CustomerDto> AddCustomerAsync(AddCustomerDto addCustomerDto);
        Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto updateCustomerDto);
        Task<bool> DeleteCustomerAsync(int id);
    }
}
