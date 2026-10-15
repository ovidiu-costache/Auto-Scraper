using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<CarListing> CarListings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var listing = modelBuilder.Entity<CarListing>();

            listing.ToTable("CarListings", "dbo", table =>
            {
                table.HasCheckConstraint("CK_CarListings_Make", "LEN(LTRIM(RTRIM(Make))) > 0");
                table.HasCheckConstraint("CK_CarListings_Model", "LEN(LTRIM(RTRIM(Model))) > 0");
                table.HasCheckConstraint("CK_CarListings_Year", "Year BETWEEN 1886 AND 9999");
                table.HasCheckConstraint("CK_CarListings_Mileage", "Mileage IS NULL OR Mileage >= 0");
                table.HasCheckConstraint("CK_CarListings_Price", "Price > 0");
                table.HasCheckConstraint("CK_CarListings_Currency", "Currency IN ('EUR', 'RON')");
                table.HasCheckConstraint("CK_CarListings_EngineCapacity", "EngineCapacity IS NULL OR EngineCapacity > 0");
                table.HasCheckConstraint("CK_CarListings_ListingUrl", "LEN(LTRIM(RTRIM(ListingUrl))) > 0");
                table.HasCheckConstraint("CK_CarListings_Source", "LEN(LTRIM(RTRIM(Source))) > 0");
            });

            listing.HasKey(c => c.Id)
                .HasName("PK_CarListings")
                .IsClustered();

            listing.Property(c => c.Id)
                .HasDefaultValueSql("NEWID()", "DF_CarListings_Id");

            listing.Property(c => c.Make).HasMaxLength(50).IsRequired();
            listing.Property(c => c.Model).HasMaxLength(50).IsRequired();
            listing.Property(c => c.Price).HasPrecision(18, 2);
            listing.Property(c => c.Currency)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .IsRequired();
            listing.Property(c => c.FuelType).HasMaxLength(30);
            listing.Property(c => c.Drivetrain).HasMaxLength(30);
            listing.Property(c => c.Transmission).HasMaxLength(30);
            listing.Property(c => c.ListingUrl).HasMaxLength(500).IsRequired();
            listing.Property(c => c.CoverImage).HasColumnType("varbinary(max)");
            listing.Property(c => c.Source).HasMaxLength(50).IsRequired();

            // SQL Server datetime2 does not preserve DateTimeKind.
            listing.Property(c => c.ScrapedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("SYSUTCDATETIME()", "DF_CarListings_ScrapedAt")
                .HasConversion(
                    value => RequireUtc(value),
                    value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

            listing.HasIndex(c => c.ListingUrl)
                .HasDatabaseName("UQ_CarListings_ListingUrl")
                .IsUnique()
                .IsClustered(false);

            listing.HasIndex(c => new { c.Make, c.Model, c.Currency, c.Year })
                .HasDatabaseName("IX_CarListings_Make_Model_Currency_Year")
                .IncludeProperties(c => new { c.Price, c.Mileage, c.EngineCapacity })
                .IsClustered(false);
        }

        private static DateTime RequireUtc(DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("ScrapedAt must have DateTimeKind.Utc.", nameof(value));
            }

            return value;
        }
    }
}
