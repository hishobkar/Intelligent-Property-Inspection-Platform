using InspectionService.Domain.Entities;
using InspectionService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InspectionService.Infrastructure.Data
{
    public class InspectionDbContext : DbContext
    {
        public InspectionDbContext(DbContextOptions<InspectionDbContext> options) : base(options) { }

        public DbSet<Inspection> Inspections => Set<Inspection>();
        public DbSet<InspectionPhoto> InspectionPhotos => Set<InspectionPhoto>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Inspection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PropertyReference).HasMaxLength(50);
                entity.Property(e => e.InspectorId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.InspectorName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(2000);
                entity.Property(e => e.Findings).HasMaxLength(5000);
                entity.HasIndex(e => e.PropertyId);
                entity.HasIndex(e => e.Status);

                entity.HasMany(e => e.Photos)
                      .WithOne(p => p.Inspection)
                      .HasForeignKey(p => p.InspectionId);

                entity.HasData(
                    new Inspection
                    {
                        Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                        PropertyId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        PropertyReference = "PROP-20260101-DEMO0001",
                        InspectorId = "inspector-001",
                        InspectorName = "John Smith",
                        Type = InspectionType.Routine,
                        Priority = InspectionPriority.Medium,
                        Status = InspectionStatus.Completed,
                        ScheduledDate = new DateTime(2026, 3, 15, 9, 0, 0, DateTimeKind.Utc),
                        CompletedDate = new DateTime(2026, 3, 15, 14, 30, 0, DateTimeKind.Utc),
                        Notes = "Routine annual inspection",
                        Findings = "Property in good condition. Minor roof repair recommended.",
                        OverallConditionScore = 78,
                        CreatedAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                        IsActive = true
                    },
                    new Inspection
                    {
                        Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                        PropertyId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        PropertyReference = "PROP-20260101-DEMO0003",
                        InspectorId = "inspector-002",
                        InspectorName = "Sarah Johnson",
                        Type = InspectionType.PrePurchase,
                        Priority = InspectionPriority.High,
                        Status = InspectionStatus.Scheduled,
                        ScheduledDate = new DateTime(2026, 6, 25, 10, 0, 0, DateTimeKind.Utc),
                        Notes = "Pre-purchase inspection required",
                        Findings = "",
                        OverallConditionScore = 0,
                        CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                        IsActive = true
                    }
                );
            });

            modelBuilder.Entity<InspectionPhoto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ContentType).HasMaxLength(100);
                entity.Property(e => e.BlobUrl).HasMaxLength(1000);
                entity.Property(e => e.Description).HasMaxLength(500);
            });
        }
    }
}
