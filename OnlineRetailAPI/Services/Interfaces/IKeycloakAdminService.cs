namespace OnlineRetailAPI.Services.Interfaces
{
    public interface IKeycloakAdminService
    {
        Task<string?> CreateClientAsync(
            string clientId,
            List<string>? redirectUris = null,
            bool publicClient = false,
            bool directAccessGrantsEnabled = false,
            bool standardFlowEnabled = true);

        Task<string?> CreateUserAsync(
            string firstName,
            string lastName,
            string email,
            string username,
            string password,
            string? phoneNumber = null,
            string? address = null);
    }
}