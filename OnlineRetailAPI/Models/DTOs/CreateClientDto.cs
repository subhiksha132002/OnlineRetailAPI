using System.ComponentModel.DataAnnotations;

namespace OnlineRetailAPI.Models.DTOs
{
    public class CreateClientDto
    {
        [Required]
        public string ClientId { get; set; } = default!;

        public List<string>? RedirectUris { get; set; }

        public bool PublicClient { get; set; } = false;

        public bool DirectAccessGrantsEnabled { get; set; } = false;

        public bool StandardFlowEnabled { get; set; } = true;
    }
}
