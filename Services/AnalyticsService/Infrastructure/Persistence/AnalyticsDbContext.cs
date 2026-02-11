using Microsoft.EntityFrameworkCore;
using AnalyticsService.Domain.Entities;

namespace AnalyticsService.Infrastructure.Persistence;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();
    public DbSet<BookingMetricDaily> BookingMetricsDaily => Set<BookingMetricDaily>();
    public DbSet<VacancyMetricMonthly> VacancyMetricsMonthly => Set<VacancyMetricMonthly>();
    public DbSet<RevenueMetricMonthly> RevenueMetricsMonthly => Set<RevenueMetricMonthly>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);

        // Global soft-delete filters
        modelBuilder.Entity<ProcessedEvent>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<BookingMetricDaily>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<VacancyMetricMonthly>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<RevenueMetricMonthly>().HasQueryFilter(x => x.DeletedAt == null);

        base.OnModelCreating(modelBuilder);
    }
}
