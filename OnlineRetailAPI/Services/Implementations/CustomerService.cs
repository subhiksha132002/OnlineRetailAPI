using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OnlineRetailAPI.Data;
using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Models.Entities;
using OnlineRetailAPI.Services.Interfaces;
using System.Text.Json;

namespace OnlineRetailAPI.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly IDistributedCache _cache;

        private readonly IKeycloakAdminService _keycloakAdminService;

        private const string AllCustomersCacheKey = "customers";
        private static string CustomerCacheKey(int id) => $"customer:{id}";


        public CustomerService(ApplicationDbContext dbContext,IDistributedCache cache, IKeycloakAdminService keycloakAdminService)
        {
            _dbContext = dbContext;
            _cache = cache;
            _keycloakAdminService = keycloakAdminService;
        }

        private async Task<T?> GetFromCacheAsync<T>(string key)
        {
            var cachedData = await _cache.GetStringAsync(key);
            return string.IsNullOrEmpty(cachedData) ? default : JsonSerializer.Deserialize<T>(cachedData);
        }


        private async Task SetToCacheAsync<T>(string key, T obj, TimeSpan expiration)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            string jsonData = JsonSerializer.Serialize(obj);
            await _cache.SetStringAsync(key, jsonData, options);
        }

        private async Task RemoveFromCacheAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }


        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var cachedCustomers = await GetFromCacheAsync<IEnumerable<CustomerDto>>(AllCustomersCacheKey);

            if (cachedCustomers != null)
                return cachedCustomers;

            var customersDto = await _dbContext.Customers.Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Email = c.Email,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber

            }).ToListAsync();

            await SetToCacheAsync(AllCustomersCacheKey, customersDto, TimeSpan.FromMinutes(5));

            return customersDto;

        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int customerId)
        {
            var cacheKey = CustomerCacheKey(customerId);
            var cachedCustomer = await GetFromCacheAsync<CustomerDto>(cacheKey);

            if (cachedCustomer != null)
                return cachedCustomer;

            var customer = await _dbContext.Customers.FindAsync(customerId);

            if (customer == null) return null;

            var customerDto =  new CustomerDto
                {
                    CustomerId = customer.CustomerId,
                    CustomerName = customer.CustomerName,
                    Email = customer.Email,
                    Address = customer.Address,
                    PhoneNumber = customer.PhoneNumber
                };

            if(customerDto != null)
            {
                await SetToCacheAsync(cacheKey, customerDto, TimeSpan.FromMinutes(5));
            }

            return customerDto;
        }

        public async Task<CustomerDto?> GetCustomerByEmailAsync(string email)
        {
            var customer = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.Email == email);

            if (customer == null)
                return null;

            return new CustomerDto
            {
                CustomerId = customer.CustomerId,
                CustomerName = customer.CustomerName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address
            };
        }

        public async Task<CustomerDto> AddCustomerAsync(AddCustomerDto addCustomerDto)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Split customer name into first and last name
                var nameParts = addCustomerDto.CustomerName.Split(' ', 2);
                var firstName = nameParts[0];
                var lastName = nameParts.Length > 1 ? nameParts[1] : "";

                // Create user in Keycloak first
                var keycloakUserId = await _keycloakAdminService.CreateUserAsync(
                    firstName,
                    lastName,
                    addCustomerDto.Email,
                    addCustomerDto.Email, // Using email as username
                    addCustomerDto.Password ?? throw new ArgumentException("Password is required"),
                    addCustomerDto.PhoneNumber,
                    addCustomerDto.Address
                );

                if (string.IsNullOrEmpty(keycloakUserId))
                {
                    throw new Exception("Failed to create user in Keycloak");
                }

                // Create customer in database
                var customerEntity = new Customer
                {
                    CustomerName = addCustomerDto.CustomerName,
                    Email = addCustomerDto.Email,
                    Password = addCustomerDto.Password,
                    Address = addCustomerDto.Address,
                    PhoneNumber = addCustomerDto.PhoneNumber,
                    KeycloakUserId = keycloakUserId
                };

                await _dbContext.Customers.AddAsync(customerEntity);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
                await RemoveFromCacheAsync(AllCustomersCacheKey);

                var customerDto = new CustomerDto
                {
                    CustomerId = customerEntity.CustomerId,
                    CustomerName = customerEntity.CustomerName,
                    Email = customerEntity.Email,
                    Address = customerEntity.Address,
                    PhoneNumber = customerEntity.PhoneNumber
                };

                return customerDto;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Failed to create customer: {ex.Message}", ex);
            }
        }


        public async Task<CustomerDto?> UpdateCustomerAsync(int customerId, UpdateCustomerDto updateCustomerDto)
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

            await RemoveFromCacheAsync(AllCustomersCacheKey);
            await RemoveFromCacheAsync(CustomerCacheKey(customerId));

            var customerDto = new CustomerDto
            {
                CustomerId = customer.CustomerId,
                CustomerName = customer.CustomerName,
                Email = customer.Email,
                Address = customer.Address,
                PhoneNumber = customer.PhoneNumber
            };

            return customerDto;
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            var customer = await _dbContext.Customers.FindAsync(customerId);

            if (customer == null)
                return false;

            _dbContext.Customers.Remove(customer);
            await _dbContext.SaveChangesAsync();

            await RemoveFromCacheAsync(AllCustomersCacheKey);
            await RemoveFromCacheAsync(CustomerCacheKey(customerId));

            return true;
        }

    }
}
