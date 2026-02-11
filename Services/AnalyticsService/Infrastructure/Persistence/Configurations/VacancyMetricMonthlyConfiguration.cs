using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AnalyticsService.Domain.Entities;

namespace AnalyticsService.Infrastructure.Persistence.Configurations;

public class VacancyMetricMonthlyConfiguration : IEntityTypeConfiguration<VacancyMetricMonthly>
{
    public void Configure(EntityTypeBuilder<VacancyMetricMonthly> builder)
    {
        builder.ToTable("vacancy_metrics_monthly");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.PropertyId);
        builder.HasIndex(x => new { x.Year, x.Month });

        // Unique constraint: one record per property/unit/year/month
        builder.HasIndex(x => new { x.PropertyId, x.UnitId, x.Year, x.Month }).IsUnique();
    }
}
