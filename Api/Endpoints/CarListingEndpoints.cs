using Application.DTOs;
using Application.Queries;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints
{
    public static class CarListingEndpoints
    {
        public static IEndpointRouteBuilder MapCarListingEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/car-listings", GetCarListingsAsync)
                .WithName("GetCarListings")
                .WithTags("CarListings")
                .WithSummary("Get a filtered page of car listings.")
                .WithDescription(
                    "Optional make and model filters use exact matching under the database collation. " +
                    "Text filters are trimmed; currency accepts EUR or RON, case-insensitively. " +
                    "Year bounds are inclusive and must be between 1886 and 9999. " +
                    "Page starts at 1; pageSize defaults to 20 and must be between 1 and 100. " +
                    "Results are ordered by ScrapedAt descending, then Id ascending. " +
                    "HasMore indicates whether another page exists at query time. " +
                    "Price is the asking price; ScrapedAt is the latest successful observation in UTC.");

            return app;
        }

        private static async Task<Results<Ok<CarListingsPageDto>, ValidationProblem>> GetCarListingsAsync(
            DbServices db,
            CancellationToken cancellationToken,
            string? make = null,
            string? model = null,
            int? minYear = null,
            int? maxYear = null,
            string? currency = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = new CarListingsQuery(make, model, minYear, maxYear, currency, page, pageSize);
            var errors = query.GetValidationErrors();
            if (errors.Count > 0)
            {
                return TypedResults.ValidationProblem(errors);
            }

            return TypedResults.Ok(await db.GetCarListingsAsync(query, cancellationToken));
        }
    }
}
