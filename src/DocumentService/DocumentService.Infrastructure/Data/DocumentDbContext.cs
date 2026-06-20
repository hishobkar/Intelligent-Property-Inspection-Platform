using DocumentService.Domain.Entities;
using DocumentService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DocumentService.Infrastructure.Data
{
    public class DocumentDbContext : DbContext
    {
        public DocumentDbContext(DbContextOptions<DocumentDbContext> options) : base(options) { }

        public DbSet<Document> Documents => Set<Document>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.BlobUrl).HasMaxLength(1000);
                entity.Property(e => e.GeneratedBy).HasMaxLength(100);
                entity.Property(e => e.ContentText).HasColumnType("TEXT");
                entity.HasIndex(e => e.PropertyId);
                entity.HasIndex(e => e.Type);
            });
        }
    }
}
