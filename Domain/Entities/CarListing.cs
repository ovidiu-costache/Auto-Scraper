using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class CarListing
    {
        public Guid Id { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Mileage { get; set; }
        public decimal Price { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public int EngineCapacity { get; set; }
        public string Transmission { get; set; } = string.Empty;
        public string ListingUrl { get; set; } = string.Empty;
        public byte[]? CoverImage { get; set; }
        public string Source { get; set; } = string.Empty;
        public DateTime ScrapedAt { get; set; }
    }
}
