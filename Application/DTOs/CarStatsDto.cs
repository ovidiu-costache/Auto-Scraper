using System;

namespace Application.DTOs
{
    public sealed record CarStatsDto(
        string Make,
        string Model,
        string Currency,
        long TotalListings,
        long MileageListings,
        long EngineCapacityListings,
        decimal? AveragePrice,
        decimal? AverageMileage,
        decimal? AverageEngineCapacity);
}
