using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<CarListing> CarListings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CarListing>().ToTable("CarListings");

            // The table is created manually with "DEFAULT NEWID()", EF Core is informed about this SQL behavior
            modelBuilder.Entity<CarListing>()
                .Property(c => c.Id)
                .HasDefaultValueSql("NEWID()");
        }
    }
}
