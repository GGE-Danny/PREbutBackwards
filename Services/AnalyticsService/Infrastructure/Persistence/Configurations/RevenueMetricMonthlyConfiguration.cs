using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AnalyticsService.Domain.Entities;

namespace AnalyticsService.Infrastructure.Persistence.Configurations;

public class RevenueMetricMonthlyConfiguration : IEntityTypeConfiguration<RevenueMetricMonthly>
{
    public void Configure(EntityTypeBuilder<RevenueMetricMonthly> builder)
    {
        builder.ToTable("revenue_metrics_monthly");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RentCollected).HasPrecision(18, 2);
        builder.Property(x => x.Expenses).HasPrecision(18, 2);
        builder.Property(x => x.NetRevenue).HasPrecision(18, 2);

        builder.HasIndex(x => x.PropertyId);
        builder.HasIndex(x => new { x.Year, x.Month });

        // Unique constraint: one record per property/year/month
        builder.HasIndex(x => new { x.PropertyId, x.Year, x.Month }).IsUnique();
    }
}
