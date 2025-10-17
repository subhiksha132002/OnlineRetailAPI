using System.Text.Json.Serialization;

namespace OnlineRetailAPI.Models.Keycloak
{
    public class KeycloakUser
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = default!;

        [JsonPropertyName("email")]
        public string Email { get; set; } = default!;

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = default!
            ;
        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = default!;

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("emailVerified")]
        public bool EmailVerified { get; set; } = false;

        [JsonPropertyName("attributes")]
        public Dictionary<string, List<string>>? Attributes { get; set; }

        [JsonPropertyName("credentials")]
        public List<KeycloakCredentials>? Credentials { get; set; }
    }
}
