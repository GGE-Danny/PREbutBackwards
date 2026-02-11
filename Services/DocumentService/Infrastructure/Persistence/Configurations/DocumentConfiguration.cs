using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DocumentService.Domain.Entities;

namespace DocumentService.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName).IsRequired().HasMaxLength(255);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.StoragePath).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ChecksumSha256).HasMaxLength(64);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        // Individual indexes
        builder.HasIndex(x => x.EntityId);
        builder.HasIndex(x => x.TenantUserId);
        builder.HasIndex(x => x.CreatedAt);

        // Composite index for common query pattern
        builder.HasIndex(x => new { x.DocumentFor, x.EntityId });

        // Unique constraint to prevent duplicate uploads of same file to same entity
        builder.HasIndex(x => new { x.DocumentFor, x.EntityId, x.ChecksumSha256 })
            .IsUnique()
            .HasFilter("\"ChecksumSha256\" IS NOT NULL");
    }
}
