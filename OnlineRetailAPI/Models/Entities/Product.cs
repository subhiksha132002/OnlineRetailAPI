using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRetailAPI.Models.Entities
{
    public class Product
    {
        public int ProductId { get; set; }

        public required string ProductName { get; set; }

        public string? ProductDescription { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public required decimal ProductPrice { get; set; }

        public required int StockQuantity { get; set; }

        public required string ImageUrl { get; set; }

    }
}
