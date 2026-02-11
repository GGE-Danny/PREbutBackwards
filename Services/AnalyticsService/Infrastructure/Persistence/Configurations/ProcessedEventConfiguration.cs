using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AnalyticsService.Domain.Entities;

namespace AnalyticsService.Infrastructure.Persistence.Configurations;

public class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.ToTable("processed_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MessageId).IsRequired().HasMaxLength(100);

        // Unique constraint on MessageId for idempotency
        builder.HasIndex(x => x.MessageId).IsUnique();
    }
}
