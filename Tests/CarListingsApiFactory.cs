using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace AutoScraper.Tests
{
    public sealed class CarListingsApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _environment;

        public CarListingsApiFactory() : this("Development") { }

        internal CarListingsApiFactory(string environment)
        {
            _environment = environment;
        }

        public static Guid ListingId(int number) =>
            Guid.Parse($"00000000-0000-0000-0000-{number:D12}");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(_environment);
            builder.UseSetting("ConnectionStrings:DefaultConnection",
                "Server=unused.invalid;Database=Unused;Integrated Security=True");

            builder.ConfigureServices(services =>
            {
                // Replace every SQL Server options registration before any context is resolved.
                services.RemoveAll<AppDbContext>();
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
                var databaseName = Guid.NewGuid().ToString();
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName));
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (!db.Database.IsInMemory())
            {
                throw new InvalidOperationException("Tests must never seed a persistent database.");
            }

            var tiedTime = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc);

            // Deliberately insert out of order, including a tie across a page boundary.
            db.CarListings.AddRange(
                CreateListing(3, "BMW", "320", 2022, "EUR", tiedTime),
                CreateListing(4, "BMW", "320", 2014, "EUR", tiedTime.AddDays(-1)),
                CreateListing(2, "BMW", "320", 2020, "EUR", tiedTime),
                CreateListing(1, "Audi", "A4", 2021, "RON", tiedTime.AddDays(1)));
            db.SaveChanges();
            return host;
        }

        private static CarListing CreateListing(
            int number, string make, string model, int year, string currency, DateTime scrapedAt) => new()
            {
                Id = ListingId(number),
                Make = make,
                Model = model,
                Year = year,
                Price = 15000,
                Currency = currency,
                ListingUrl = $"https://example.invalid/listings/{number}",
                CoverImage = [1, 2, 3],
                Source = "Test",
                ScrapedAt = scrapedAt
            };
    }
}
