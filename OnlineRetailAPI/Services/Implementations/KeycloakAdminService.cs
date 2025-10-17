using FS.Keycloak.RestApiClient.Api;
using FS.Keycloak.RestApiClient.Authentication.ClientFactory;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using FS.Keycloak.RestApiClient.ClientFactory;
using FS.Keycloak.RestApiClient.Model;
using OnlineRetailAPI.Services.Interfaces;

namespace OnlineRetailAPI.Services.Implementations
{
    public class KeycloakAdminService : IKeycloakAdminService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KeycloakAdminService> _logger;
        private readonly string _adminUrl;
        private readonly string _realm;
        private readonly string _serviceAccountClientId;
        private readonly string _serviceAccountClientSecret;

        public KeycloakAdminService(IConfiguration configuration, ILogger<KeycloakAdminService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _adminUrl = _configuration["Keycloak:AdminUrl"] ?? throw new InvalidOperationException("Keycloak:AdminUrl not configured");
            _realm = _configuration["Keycloak:Realm"] ?? throw new InvalidOperationException("Keycloak:Realm not configured");
            _serviceAccountClientId = _configuration["Keycloak:ServiceAccount:ClientId"] ?? throw new InvalidOperationException("Keycloak:ServiceAccount:ClientId not configured");
            _serviceAccountClientSecret = _configuration["Keycloak:ServiceAccount:ClientSecret"] ?? throw new InvalidOperationException("Keycloak:ServiceAccount:ClientSecret not configured");


            _logger.LogInformation("KeycloakAdminService initialized");
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
                var keycloakUrl = _adminUrl;
                var realm = _realm;
                var serviceClientId = _serviceAccountClientId;
                var serviceClientSecret = _serviceAccountClientSecret;

                _logger.LogInformation("Creating client with URL: {Url}, Realm: {Realm}", keycloakUrl, realm);

                var credentials = new ClientCredentialsFlow
                {
                    KeycloakUrl = keycloakUrl,
                    Realm = realm,
                    ClientId = serviceClientId,
                    ClientSecret = serviceClientSecret
                };

                using var httpClient = AuthenticationHttpClientFactory.Create(credentials);
                using var clientsApi = ApiClientFactory.Create<ClientsApi>(httpClient);

                var clientRepresentation = new ClientRepresentation
                {
                    ClientId = clientId,
                    Enabled = true,
                    PublicClient = publicClient,
                    RedirectUris = redirectUris,
                    DirectAccessGrantsEnabled = directAccessGrantsEnabled,
                    StandardFlowEnabled = standardFlowEnabled
                };

                _logger.LogInformation("Creating client: {ClientId}", clientId);


                await clientsApi.PostClientsAsync(realm, clientRepresentation);

               
                var clients = await clientsApi.GetClientsAsync(realm, clientId: clientId);

                if (clients != null && clients.Count > 0)
                {
                    var createdClient = clients[0];
                    _logger.LogInformation("Client created successfully with ID: {Id}", createdClient.Id);
                    return createdClient.Id;
                }

                _logger.LogWarning("Client created but could not retrieve ID");
                return null;
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
                var keycloakUrl = _adminUrl;
                var realm = _realm;
                var serviceClientId = _serviceAccountClientId;
                var serviceClientSecret = _serviceAccountClientSecret;

                _logger.LogInformation("Creating user with URL: {Url}, Realm: {Realm}", keycloakUrl, realm);

                var credentials = new ClientCredentialsFlow
                {
                    KeycloakUrl = keycloakUrl,
                    Realm = realm,
                    ClientId = serviceClientId,
                    ClientSecret = serviceClientSecret
                };

                using var httpClient = AuthenticationHttpClientFactory.Create(credentials);
                using var usersApi = ApiClientFactory.Create<UsersApi>(httpClient);

                // custom attributes
                var attributes = new Dictionary<string, List<string>>();
                if (!string.IsNullOrEmpty(phoneNumber))
                    attributes["phoneNumber"] = new List<string> { phoneNumber };
                if (!string.IsNullOrEmpty(address))
                    attributes["address"] = new List<string> { address };

                var userRepresentation = new UserRepresentation
                {
                    Username = username,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Enabled = true,
                    EmailVerified = false,
                    Attributes = attributes.Count > 0 ? attributes : null,
                    Credentials = new List<CredentialRepresentation>
                    {
                        new CredentialRepresentation
                        {
                            Type = "password",
                            Value = password,
                            Temporary = false
                        }
                    }
                };

                _logger.LogInformation("Creating user: {Username}", username);

                
                await usersApi.PostUsersAsync(realm, userRepresentation);

                
                var users = await usersApi.GetUsersAsync(realm, username: username, exact: true);

                if (users != null && users.Count > 0)
                {
                    var createdUser = users[0];
                    _logger.LogInformation("User created successfully with ID: {Id}", createdUser.Id);
                    return createdUser.Id;
                }

                _logger.LogWarning("User created but could not retrieve ID");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user in Keycloak: {Message}", ex.Message);
                throw new Exception($"Error creating user in Keycloak: {ex.Message}", ex);
            }
        }
    }
}