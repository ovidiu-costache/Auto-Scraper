using System;

namespace Application.DTOs
{
    public sealed record CarListingDto(
        Guid Id,
        string Make,
        string Model,
        int Year,
        int? Mileage,
        decimal Price,
        string Currency,
        string? FuelType,
        int? EngineCapacity,
        string? Drivetrain,
        string? Transmission,
        string ListingUrl,
        byte[]? CoverImage,
        string Source,
        DateTime ScrapedAt);
}
