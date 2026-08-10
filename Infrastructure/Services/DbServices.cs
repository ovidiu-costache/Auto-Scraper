using Application.DTOs;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class DbServices
    {
        private readonly AppDbContext _db;

        public DbServices(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<CarListingDto>> GetAllCarListingsAsync()
        {
            return await _db.Database
                .SqlQuery<CarListingDto>($"SELECT * FROM CarListings")
                .ToListAsync();
        }

        public async Task<List<CarListingDto>> GetCarsAsync(string make, string model)
        {
            return await _db.Database
                .SqlQuery<CarListingDto>($"EXEC sp_GetCars @Make = {make}, @Model = {model}")
                .ToListAsync();
        }

        public async Task<CarStatsDto?> GetStatsAsync(string make, string model, int minYear, int maxYear)
        {
            // sp_GetCarStats always returns exactly one row with the aggregated data
            return await _db.Database
                .SqlQuery<CarStatsDto>($"EXEC sp_GetCarStats @Make = {make}, @Model = {model}, @MinYear = {minYear}, @MaxYear = {maxYear}")
                .SingleOrDefaultAsync();
        }
    }
}
