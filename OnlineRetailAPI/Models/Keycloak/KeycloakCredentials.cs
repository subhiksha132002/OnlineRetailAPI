using System.Text.Json.Serialization;

namespace OnlineRetailAPI.Models.Keycloak
{
    public class KeycloakCredentials
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "password";

        [JsonPropertyName("value")]
        public string Value { get; set; } = default!;

        [JsonPropertyName("temporary")]
        public bool Temporary { get; set; } = false;
    }
}
