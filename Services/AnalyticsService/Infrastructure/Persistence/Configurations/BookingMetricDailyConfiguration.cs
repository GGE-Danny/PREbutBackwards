using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AnalyticsService.Domain.Entities;

namespace AnalyticsService.Infrastructure.Persistence.Configurations;

public class BookingMetricDailyConfiguration : IEntityTypeConfiguration<BookingMetricDaily>
{
    public void Configure(EntityTypeBuilder<BookingMetricDaily> builder)
    {
        builder.ToTable("booking_metrics_daily");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.PropertyId);
        builder.HasIndex(x => x.Date);

        // Unique constraint: one record per property/unit/date
        builder.HasIndex(x => new { x.PropertyId, x.UnitId, x.Date }).IsUnique();
    }
}
