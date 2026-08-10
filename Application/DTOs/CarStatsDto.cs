using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public sealed record CarStatsDto(
        string Make,
        string Model,
        int TotalListings,
        decimal AveragePrice,
        int AverageMileage,
        int AverageEngineCapacity);
}
