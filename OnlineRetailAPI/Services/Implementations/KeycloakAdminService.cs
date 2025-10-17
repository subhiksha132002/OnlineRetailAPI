using OnlineRetailAPI.Models.Keycloak;
using OnlineRetailAPI.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OnlineRetailAPI.Services.Implementations
{
    public class KeycloakAdminService : IKeycloakAdminService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _adminUrl;
        private readonly string _realm;
        private readonly string _serviceAccountClientId;
        private readonly string _serviceAccountClientSecret;
        private readonly ILogger<KeycloakAdminService> _logger;

        public KeycloakAdminService(HttpClient httpClient, IConfiguration configuration, ILogger<KeycloakAdminService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _adminUrl = _configuration["Keycloak:AdminUrl"] ?? throw new InvalidOperationException("Keycloak:AdminUrl not configured");
            _realm = _configuration["Keycloak:Realm"] ?? throw new InvalidOperationException("Keycloak:Realm not configured");
            _serviceAccountClientId = _configuration["Keycloak:ServiceAccount:ClientId"] ?? throw new InvalidOperationException("Keycloak:ServiceAccount:ClientId not configured");
            _serviceAccountClientSecret = _configuration["Keycloak:ServiceAccount:ClientSecret"] ?? throw new InvalidOperationException("Keycloak:ServiceAccount:ClientSecret not configured");
        }

        private async Task<string> GetServiceAccountTokenAsync()
        {
            try
            {
                // Use the realm's token endpoint (not master realm)
                var tokenUrl = $"{_adminUrl}/realms/{_realm}/protocol/openid-connect/token";

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", _serviceAccountClientId),
                    new KeyValuePair<string, string>("client_secret", _serviceAccountClientSecret),
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

                var response = await _httpClient.PostAsync(tokenUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get service account token. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, responseContent);
                    throw new Exception($"Failed to get service account token. Status: {response.StatusCode}");
                }

                var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var accessToken = tokenResponse.GetProperty("access_token").GetString();

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("Access token is null or empty");
                }

                _logger.LogInformation("Successfully obtained service account token");
                return accessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service account token: {Message}", ex.Message);
                throw new Exception($"Error getting service account token: {ex.Message}", ex);
            }
        }

        public async Task<string?> CreateClientAsync(
            string clientId,
            List<string>? redirectUris = null,
            bool publicClient = false,
            bool directAccessGrantsEnabled = false,
            bool standardFlowEnabled = true)
        {
            try
            {
                var token = await GetServiceAccountTokenAsync();
                var url = $"{_adminUrl}/admin/realms/{_realm}/clients";

                var clientPayload = new KeycloakClient
                {
                    ClientId = clientId,
                    Enabled = true,
                    PublicClient = publicClient,
                    RedirectUris = redirectUris,
                    DirectAccessGrantsEnabled = directAccessGrantsEnabled,
                    StandardFlowEnabled = standardFlowEnabled
                };

                var jsonContent = JsonSerializer.Serialize(clientPayload, new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("Creating client with payload: {Payload}", jsonContent);

                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(url, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var locationHeader = response.Headers.Location?.ToString();
                    _logger.LogInformation("Client creation Location header: {Location}", locationHeader);

                    if (locationHeader != null)
                    {
                        var createdClientId = locationHeader.Split('/').Last();
                        _logger.LogInformation("Created client ID: {ClientId}", createdClientId);
                        return createdClientId;
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create client. Status: {StatusCode}, Response: {Error}",
                    response.StatusCode, errorContent);
                throw new Exception($"Failed to create client in Keycloak: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client in Keycloak: {Message}", ex.Message);
                throw new Exception($"Error creating client in Keycloak: {ex.Message}", ex);
            }
        }

        public async Task<string?> CreateUserAsync(
            string firstName,
            string lastName,
            string email,
            string username,
            string password,
            string? phoneNumber = null,
            string? address = null)
        {
            try
            {
                var token = await GetServiceAccountTokenAsync();
                var url = $"{_adminUrl}/admin/realms/{_realm}/users";

                var attributes = new Dictionary<string, List<string>>();
                if (!string.IsNullOrEmpty(phoneNumber))
                    attributes["phoneNumber"] = new List<string> { phoneNumber };
                if (!string.IsNullOrEmpty(address))
                    attributes["address"] = new List<string> { address };

                var userPayload = new KeycloakUser
                {
                    Username = username,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Enabled = true,
                    EmailVerified = false,
                    Attributes = attributes.Count > 0 ? attributes : null,
                    Credentials = new List<KeycloakCredentials>
                    {
                        new KeycloakCredentials
                        {
                            Type = "password",
                            Value = password,
                            Temporary = false
                        }
                    }
                };

                var jsonContent = JsonSerializer.Serialize(userPayload);
                _logger.LogInformation("User payload JSON: {Payload}", jsonContent);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.PostAsync(url, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var locationHeader = response.Headers.Location?.ToString();
                    _logger.LogInformation("Location header received: {LocationHeader}", locationHeader);

                    if (locationHeader != null)
                    {
                        var userId = locationHeader.Split('/').Last();
                        _logger.LogInformation("Extracted User ID: {UserId}", userId);
                        return userId;
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create user. Status: {StatusCode}, Response: {Error}",
                    response.StatusCode, errorContent);
                throw new Exception($"Failed to create user in Keycloak: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user in Keycloak: {Message}", ex.Message);
                throw new Exception($"Error creating user in Keycloak: {ex.Message}", ex);
            }
        }
    }
}