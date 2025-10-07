using Microsoft.AspNetCore.Mvc;
using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Services.Interfaces;


namespace OnlineRetailAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            this._customerService = customerService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var allCustomers = await _customerService.GetAllCustomersAsync();
            return Ok(allCustomers);
        }

        [HttpGet("{customerId:int}")]
        public async Task<IActionResult> GetCustomerById(int customerId)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound();

            return Ok(customer);
        }

        [HttpPost("CreateCustomer")]
        public async Task<IActionResult> AddCustomer(AddCustomerDto addCustomerDto)
        {
            var newCustomer = await _customerService.AddCustomerAsync(addCustomerDto);

            return CreatedAtAction(
                nameof(GetCustomerById),
                new { customerId = newCustomer.CustomerId },
                newCustomer
            );
        }

        [HttpPut("{customerId:int}/UpdateCustomer")]
        public async Task<IActionResult> UpdateCustomer(int customerId, UpdateCustomerDto updateCustomerDto)
        {
            var updatedCustomer = await _customerService.UpdateCustomerAsync(customerId, updateCustomerDto);

            if (updatedCustomer == null)
                return NotFound();

            return Ok(updatedCustomer);
        }

        [HttpDelete("{customerId:int}/DeleteCustomer")]
        public async Task<IActionResult> DeleteCustomer(int customerId)
        {
            var deleted = await _customerService.DeleteCustomerAsync(customerId);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
