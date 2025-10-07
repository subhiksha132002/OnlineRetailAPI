using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRetailAPI.Data;
using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Models.Entities;
using System.Net;

namespace OnlineRetailAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public CustomersController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var allCustomers = await dbContext.Customers.Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Email = c.Email,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber

            }).ToListAsync();

            return Ok(allCustomers);
        }

        [HttpGet("{customerId:int}")]
        public async Task<IActionResult> GetCustomerById(int customerId)
        {
            var customer = await dbContext.Customers.Where(c => c.CustomerId == customerId).Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Email = c.Email,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber
            }).FirstOrDefaultAsync();

            if (customer is null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        [HttpPost("CreateCustomer")]
        public async Task<IActionResult> AddCustomer(AddCustomerDto addCustomerDto)
        {
            var customerEntity = new Customer()
            {
                CustomerName = addCustomerDto.CustomerName,
                Email = addCustomerDto.Email,
                Password = addCustomerDto.Password,
                Address = addCustomerDto.Address,
                PhoneNumber = addCustomerDto.PhoneNumber
            };

            await dbContext.Customers.AddAsync(customerEntity);
            await dbContext.SaveChangesAsync();

            var customer = await dbContext.Customers.Where(c => c.CustomerId == customerEntity.CustomerId).Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Email = c.Email,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber
            }).FirstOrDefaultAsync();

            if(customer is null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetCustomerById),new { customerId = customer.CustomerId },customer);
        }

        [HttpPut("{customerId:int}/UpdateCustomer")]
        public async Task<IActionResult> UpdateCustomer(int customerId, UpdateCustomerDto updateCustomerDto)
        {
            var customer = await dbContext.Customers.FindAsync(customerId);

            if (customer is null)
            {
                return NotFound();
            }

            customer.CustomerName = updateCustomerDto.CustomerName;
            customer.Email = updateCustomerDto.Email;
            customer.Password = updateCustomerDto.Password;
            customer.Address = updateCustomerDto.Address;
            customer.PhoneNumber = updateCustomerDto.PhoneNumber;
            await dbContext.SaveChangesAsync();

            return Ok(customer);
        }

        [HttpDelete("{customerId:int}/DeleteCustomer")]
        public async Task<IActionResult> DeleteCustomer(int customerId)
        {
            var customer = await dbContext.Customers.FindAsync(customerId);

            if (customer is null)
            {
                return NotFound();

            }

            dbContext.Customers.Remove(customer);

            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
