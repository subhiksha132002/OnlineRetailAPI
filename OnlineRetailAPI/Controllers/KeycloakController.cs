
using Microsoft.AspNetCore.Mvc;
using OnlineRetailAPI.Models.DTOs;
using OnlineRetailAPI.Services.Interfaces;

namespace OnlineRetailAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeycloakController : ControllerBase
    {
        private readonly IKeycloakAdminService _keycloakAdminService;
        private readonly ILogger<KeycloakController> _logger;

        public KeycloakController(IKeycloakAdminService keycloakAdminService, ILogger<KeycloakController> logger)
        {
            _keycloakAdminService = keycloakAdminService;
            _logger = logger;
        }

        [HttpGet("clients/{clientId}")]
        public async Task<IActionResult> GetClientById(string clientId)
        {
            return Ok(new { Message = $"Would return client details for {clientId}" });
        }

        [HttpPost("clients")]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientDto createClientDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdClientId = await _keycloakAdminService.CreateClientAsync(
                    clientId: createClientDto.ClientId,
                    redirectUris: createClientDto.RedirectUris,
                    publicClient: createClientDto.PublicClient,
                    directAccessGrantsEnabled: createClientDto.DirectAccessGrantsEnabled,
                    standardFlowEnabled: createClientDto.StandardFlowEnabled
                );

                if (string.IsNullOrEmpty(createdClientId))
                    return StatusCode(500, "Client created but no ID returned from Keycloak.");

                return CreatedAtAction(
                    nameof(GetClientById),
                    new { clientId = createdClientId },
                    new
                    {
                        Id = createdClientId,
                        createClientDto.ClientId
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Keycloak client: {Message}", ex.Message);
                return StatusCode(500, $"Failed to create client: {ex.Message}");
            }
        }

    }
}
