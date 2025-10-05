namespace OnlineRetailAPI.Models
{
    public class UpdateProductDto
    {
        public required string ProductName { get; set; }

        public string? ProductDescription { get; set; }

        public required decimal ProductPrice { get; set; }

        public required int StockQuantity { get; set; }

        public required string ImageUrl { get; set; }

    }
}
