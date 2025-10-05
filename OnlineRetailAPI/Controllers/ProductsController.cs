using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineRetailAPI.Data;

namespace OnlineRetailAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        public ProductsController(ApplicationDbContext)
        {
            
        }
        [HttpGet]
        public IActionResult GetAllProducts()
        {
             
        }
    }
}
