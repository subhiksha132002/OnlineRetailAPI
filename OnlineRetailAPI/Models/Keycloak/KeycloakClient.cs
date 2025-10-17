namespace OnlineRetailAPI.Models.Keycloak
{
    public class KeycloakClient
    {
        public string ClientId { get; set; } = default!;
        public string? Name { get; set; }
        public bool Enabled { get; set; } = true;
        public bool PublicClient { get; set; } = false;
        public string Protocol { get; set; } = "openid-connect";
        public List<string>? RedirectUris { get; set; }
        public bool? DirectAccessGrantsEnabled { get; set; }
        public bool? StandardFlowEnabled { get; set; }
    }
}
