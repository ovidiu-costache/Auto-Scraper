using Application.DTOs;
using Application.Queries;
using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Services
{
    public class DbServices
    {
        private readonly AppDbContext _db;

        public DbServices(AppDbContext db)
        {
            _db = db;
        }

        public async Task<CarListingsPageDto> GetCarListingsAsync(
            CarListingsQuery query, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            var listings = _db.CarListings.AsNoTracking();

            if (query.Make is not null)
            {
                listings = listings.Where(c => c.Make == query.Make);
            }

            if (query.Model is not null)
            {
                listings = listings.Where(c => c.Model == query.Model);
            }

            if (query.MinYear.HasValue)
            {
                listings = listings.Where(c => c.Year >= query.MinYear.Value);
            }

            if (query.MaxYear.HasValue)
            {
                listings = listings.Where(c => c.Year <= query.MaxYear.Value);
            }

            if (query.Currency is not null)
            {
                listings = listings.Where(c => c.Currency == query.Currency);
            }

            var offset = checked((int)(((long)query.Page - 1) * query.PageSize));
            var items = await listings
                .OrderByDescending(c => c.ScrapedAt)
                .ThenBy(c => c.Id)
                .Skip(offset)
                .Take(query.PageSize + 1)
                .Select(c => new CarListingListItemDto(
                    c.Id,
                    c.Make,
                    c.Model,
                    c.Year,
                    c.Mileage,
                    c.Price,
                    c.Currency,
                    c.FuelType,
                    c.EngineCapacity,
                    c.Drivetrain,
                    c.Transmission,
                    c.ListingUrl,
                    c.Source,
                    c.ScrapedAt))
                .ToListAsync(cancellationToken);

            var hasMore = items.Count > query.PageSize;
            if (hasMore)
            {
                items.RemoveAt(items.Count - 1);
            }

            return new CarListingsPageDto(items, query.Page, query.PageSize, hasMore);
        }

        public async Task<CarStatsDto> GetStatsAsync(
            string make, string model, int minYear, int maxYear, string currency,
            CancellationToken cancellationToken = default)
        {
            ValidateMakeAndModel(make, model);
            ArgumentOutOfRangeException.ThrowIfLessThan(minYear, 1886);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(minYear, 9999);
            ArgumentOutOfRangeException.ThrowIfLessThan(maxYear, 1886);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(maxYear, 9999);

            if (minYear > maxYear)
            {
                throw new ArgumentException("MinYear must not exceed MaxYear.", nameof(minYear));
            }

            if (currency is not ("EUR" or "RON"))
            {
                throw new ArgumentException("Currency must be EUR or RON.", nameof(currency));
            }

            var makeParameter = new SqlParameter("make", SqlDbType.NVarChar, 50) { Value = make };
            var modelParameter = new SqlParameter("model", SqlDbType.NVarChar, 50) { Value = model };
            var minYearParameter = new SqlParameter("minYear", SqlDbType.Int) { Value = minYear };
            var maxYearParameter = new SqlParameter("maxYear", SqlDbType.Int) { Value = maxYear };
            var currencyParameter = new SqlParameter("currency", SqlDbType.Char, 3) { Value = currency };

            // Materialize before Single: SQL Server cannot compose queries over EXEC.
            var results = await _db.Database
                .SqlQuery<CarStatsDto>($"EXEC dbo.GetCarStats @Make = {makeParameter}, @Model = {modelParameter}, @MinYear = {minYearParameter}, @MaxYear = {maxYearParameter}, @Currency = {currencyParameter}")
                .ToListAsync(cancellationToken);

            return results.Single();
        }

        private static void ValidateMakeAndModel(string make, string model)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(make);
            ArgumentException.ThrowIfNullOrWhiteSpace(model);

            if (make.Length > 50)
            {
                throw new ArgumentException("Make must not exceed 50 characters.", nameof(make));
            }

            if (model.Length > 50)
            {
                throw new ArgumentException("Model must not exceed 50 characters.", nameof(model));
            }
        }
    }
}
