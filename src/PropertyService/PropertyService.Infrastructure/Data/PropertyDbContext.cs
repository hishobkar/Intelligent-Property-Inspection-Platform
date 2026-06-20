using Microsoft.EntityFrameworkCore;
using PropertyService.Domain.Entities;

namespace PropertyService.Infrastructure.Data
{
    public class PropertyDbContext : DbContext
    {
        public PropertyDbContext(DbContextOptions<PropertyDbContext> options) : base(options) { }

        public DbSet<Property> Properties => Set<Property>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Property>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PropertyReference).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.State).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PostalCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
                entity.Property(e => e.OwnerId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EstimatedValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SquareFootage).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.PropertyReference).IsUnique();
                entity.HasIndex(e => e.OwnerId);

                entity.HasData(
                    new Property
                    {
                        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        PropertyReference = "PROP-20260101-DEMO0001",
                        Address = "123 Main Street",
                        City = "New York",
                        State = "NY",
                        PostalCode = "10001",
                        Country = "USA",
                        Type = Domain.Enums.PropertyType.Residential,
                        YearBuilt = 1995,
                        SquareFootage = 1850,
                        Bedrooms = 3,
                        Bathrooms = 2,
                        EstimatedValue = 650000,
                        OwnerId = "owner-001",
                        Status = Domain.Enums.PropertyStatus.Available,
                        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        IsActive = true
                    },
                    new Property
                    {
                        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        PropertyReference = "PROP-20260101-DEMO0002",
                        Address = "456 Oak Avenue",
                        City = "Los Angeles",
                        State = "CA",
                        PostalCode = "90210",
                        Country = "USA",
                        Type = Domain.Enums.PropertyType.Commercial,
                        YearBuilt = 2005,
                        SquareFootage = 5200,
                        Bedrooms = 0,
                        Bathrooms = 4,
                        EstimatedValue = 2100000,
                        OwnerId = "owner-002",
                        Status = Domain.Enums.PropertyStatus.Available,
                        CreatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                        IsActive = true
                    },
                    new Property
                    {
                        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        PropertyReference = "PROP-20260101-DEMO0003",
                        Address = "789 Pine Road",
                        City = "Chicago",
                        State = "IL",
                        PostalCode = "60601",
                        Country = "USA",
                        Type = Domain.Enums.PropertyType.Residential,
                        YearBuilt = 1978,
                        SquareFootage = 2100,
                        Bedrooms = 4,
                        Bathrooms = 2,
                        EstimatedValue = 425000,
                        OwnerId = "owner-001",
                        Status = Domain.Enums.PropertyStatus.UnderInspection,
                        CreatedAt = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc),
                        IsActive = true
                    }
                );
            });
        }
    }
}
