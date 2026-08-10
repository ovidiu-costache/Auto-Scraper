using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public sealed record CarListingDto(
        Guid Id,
        string Make,
        string Model,
        int Year,
        int Mileage,
        decimal Price,
        string FuelType,
        int EngineCapacity,
        string Transmission,
        string ListingUrl,
        byte[]? CoverImage,
        string Source,
        DateTime ScrapedAt);
}
