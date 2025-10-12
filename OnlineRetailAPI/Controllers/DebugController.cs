using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace OnlineRetailAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController :ControllerBase
    {

        // Test 1: Just authentication (no role required)
        [HttpGet("test-auth")]
        [Authorize]
        public IActionResult TestAuth()
        {
            return Ok(new
            {
                Message = "Authentication works!",
                Username = User.Identity?.Name,
                IsAuthenticated = User.Identity?.IsAuthenticated
            });
        }

        // Test 2: Check all claims
        [HttpGet("claims")]
        [Authorize]
        public IActionResult GetClaims()
        {
            var claims = User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            }).ToList();

            var roleClaims = User.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type.Contains("role", StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Value)
                .ToList();

            return Ok(new
            {
                AllClaims = claims,
                RoleClaims = roleClaims,
                HasAdminRole = User.IsInRole("admin-onlineretail"),
                HasCustomerRole = User.IsInRole("customer-onlineretail"),
                Username = User.Identity?.Name
            });
        }

        // Test 3: Admin role required
        [HttpGet("test-admin")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult TestAdmin()
        {
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return Ok(new
            {
                Message = "Admin authorization works!",
                Username = User.Identity?.Name,
                Roles = roles
            });
        }

        // Test 4: Simple role check without policy
        [HttpGet("test-role-direct")]
        [Authorize(Roles = "admin-onlineretail")]
        public IActionResult TestRoleDirect()
        {
            return Ok(new
            {
                Message = "Direct role check works!",
                Username = User.Identity?.Name
            });
        }
    }
}


