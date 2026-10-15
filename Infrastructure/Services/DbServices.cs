using Application.DTOs;
using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
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

        public async Task<List<CarListingDto>> GetAllCarListingsAsync(CancellationToken cancellationToken = default)
        {
            return await _db.CarListings
                .AsNoTracking()
                .Select(c => new CarListingDto(
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
                    c.CoverImage,
                    c.Source,
                    c.ScrapedAt))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CarListingDto>> GetCarsAsync(
            string make, string model, CancellationToken cancellationToken = default)
        {
            ValidateMakeAndModel(make, model);

            return await _db.CarListings
                .AsNoTracking()
                .Where(c => c.Make == make && c.Model == model)
                .Select(c => new CarListingDto(
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
                    c.CoverImage,
                    c.Source,
                    c.ScrapedAt))
                .ToListAsync(cancellationToken);
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
