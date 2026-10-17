using Application.DTOs;
using Application.Queries;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi;

namespace Api.Endpoints
{
    public static class CarStatsEndpoints
    {
        public static IEndpointRouteBuilder MapCarStatsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/car-stats", GetCarStatsAsync)
                .WithName("GetCarStats")
                .WithTags("CarStats")
                .WithSummary("Get car listing statistics for a make, model, year range and currency.")
                .WithDescription(
                    "All five query parameters are required. Make and model must contain 1 to 50 characters " +
                    "after trimming and use exact matching under the database collation. " +
                    "Currency is trimmed and accepts EUR or RON, case-insensitively; no currency conversion is performed. " +
                    "Year bounds are inclusive, must be between 1886 and 9999, and minYear must not exceed maxYear. " +
                    "Statistics cover stored listings and asking prices, not completed transactions or confirmed sales. " +
                    "AverageMileage and AverageEngineCapacity exclude missing values; their respective counts " +
                    "are MileageListings and EngineCapacityListings. Averages are rounded to two decimal places. " +
                    "No matching listings returns 200 with zero counts and null averages. " +
                    "Invalid input returns 400 application/problem+json.")
                .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
                {
                    // Swagger UI cannot bind required numeric query parameters with an integer/string union schema.
                    foreach (var parameter in operation.Parameters ?? [])
                    {
                        if (parameter is OpenApiParameter { Name: "minYear" or "maxYear" } yearParameter)
                        {
                            yearParameter.Schema = new OpenApiSchema
                            {
                                Type = JsonSchemaType.Integer,
                                Format = "int32"
                            };
                        }
                    }

                    return Task.CompletedTask;
                });

            return app;
        }

        private static async Task<Results<Ok<CarStatsDto>, ValidationProblem>> GetCarStatsAsync(
            DbServices db,
            CancellationToken cancellationToken,
            string make,
            string model,
            int minYear,
            int maxYear,
            string currency)
        {
            var query = new CarStatsQuery(make, model, minYear, maxYear, currency);
            var errors = query.GetValidationErrors();
            if (errors.Count > 0)
            {
                return TypedResults.ValidationProblem(errors);
            }

            return TypedResults.Ok(await db.GetStatsAsync(
                query.Make, query.Model, query.MinYear, query.MaxYear, query.Currency, cancellationToken));
        }
    }
}
