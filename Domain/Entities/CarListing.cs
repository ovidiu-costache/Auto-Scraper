namespace Domain.Entities
{
    public class CarListing
    {
        public Guid Id { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public int? Mileage { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? FuelType { get; set; }
        public int? EngineCapacity { get; set; }
        public string? Drivetrain { get; set; }
        public string? Transmission { get; set; }
        public string ListingUrl { get; set; } = string.Empty;
        public byte[]? CoverImage { get; set; }
        public string Source { get; set; } = string.Empty;
        // UTC time of the latest successful observation; refresh explicitly on update.
        public DateTime ScrapedAt { get; set; }
    }
}
