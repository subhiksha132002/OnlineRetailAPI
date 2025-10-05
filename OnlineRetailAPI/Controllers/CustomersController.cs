using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineRetailAPI.Data;
using OnlineRetailAPI.Models;
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
        public IActionResult GetAllCustomers()
        {
            var allCustomers = dbContext.Customers.ToList();

            return Ok(allCustomers);
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetCustomerById(int id)
        {
            var customer = dbContext.Customers.Find(id);

            if (customer is null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        [HttpPost]
        public IActionResult AddCustomer(AddCustomerDto addCustomerDto)
        {
            var customerEntity = new Customer()
            {
                CustomerName = addCustomerDto.CustomerName,
                Email = addCustomerDto.Email,
                Password = addCustomerDto.Password,
                Address = addCustomerDto.Address,
                PhoneNumber = addCustomerDto.PhoneNumber
            };

            dbContext.Customers.Add(customerEntity);
            dbContext.SaveChanges();

            return Ok(customerEntity);
        }

        [HttpPut]
        [Route("{id:int}")]
        public IActionResult UpdateCustomer(int id, UpdateCustomerDto updateCustomerDto)
        {
            var customer = dbContext.Customers.Find(id);

            if (customer is null)
            {
                return NotFound();
            }

            customer.CustomerName = updateCustomerDto.CustomerName;
            customer.Email = updateCustomerDto.Email;
            customer.Password = updateCustomerDto.Password;
            customer.Address = updateCustomerDto.Address;
            customer.PhoneNumber = updateCustomerDto.PhoneNumber;
            dbContext.SaveChanges();

            return Ok(customer);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteCustomer(int id)
        {
            var customer = dbContext.Customers.Find(id);

            if (customer is null)
            {
                return NotFound();

            }

            dbContext.Customers.Remove(customer);

            dbContext.SaveChanges();

            return Ok();
        }
    }
}
