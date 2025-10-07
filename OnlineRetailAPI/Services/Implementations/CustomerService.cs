using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRetailAPI.Data;
using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Models.Entities;
using OnlineRetailAPI.Services.Interfaces;

namespace OnlineRetailAPI.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _dbContext;

        public CustomerService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            return await _dbContext.Customers.Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Email = c.Email,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber

            }).ToListAsync();

        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int customerId)
        {
            return await _dbContext.Customers
                .Where(c => c.CustomerId == customerId)
                .Select(c => new CustomerDto
                {
                    CustomerId = c.CustomerId,
                    CustomerName = c.CustomerName,
                    Email = c.Email,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber
                }).FirstOrDefaultAsync();
        }

        public async Task<CustomerDto> AddCustomerAsync(AddCustomerDto addCustomerDto)
        {
            var customerEntity = new Customer
            {
                CustomerName = addCustomerDto.CustomerName,
                Email = addCustomerDto.Email,
                Password = addCustomerDto.Password,
                Address = addCustomerDto.Address,
                PhoneNumber = addCustomerDto.PhoneNumber
            };

            await _dbContext.Customers.AddAsync(customerEntity);
            await _dbContext.SaveChangesAsync();

            return new CustomerDto
            {
                CustomerId = customerEntity.CustomerId,
                CustomerName = customerEntity.CustomerName,
                Email = customerEntity.Email,
                Address = customerEntity.Address,
                PhoneNumber = customerEntity.PhoneNumber
            };
        }

        public async Task<Customer?> UpdateCustomerAsync(int customerId, UpdateCustomerDto updateCustomerDto)
        {
            var customer = await _dbContext.Customers.FindAsync(customerId);

            if (customer == null)
                return null;

            customer.CustomerName = updateCustomerDto.CustomerName;
            customer.Email = updateCustomerDto.Email;
            customer.Password = updateCustomerDto.Password;
            customer.Address = updateCustomerDto.Address;
            customer.PhoneNumber = updateCustomerDto.PhoneNumber;

            await _dbContext.SaveChangesAsync();

            return customer;
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            var customer = await _dbContext.Customers.FindAsync(customerId);

            if (customer == null)
                return false;

            _dbContext.Customers.Remove(customer);
            await _dbContext.SaveChangesAsync();

            return true;
        }

    }
}
