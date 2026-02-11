using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Message).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.MetadataJson).HasMaxLength(4000);
        builder.Property(x => x.FailedReason).HasMaxLength(1000);

        // Individual indexes
        builder.HasIndex(x => x.RecipientUserId);
        builder.HasIndex(x => x.IsRead);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.Status);

        // Composite index for common query pattern
        builder.HasIndex(x => new { x.RecipientUserId, x.IsRead, x.CreatedAt });
    }
}
