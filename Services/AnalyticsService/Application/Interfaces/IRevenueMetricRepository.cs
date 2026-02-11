using AnalyticsService.Domain.Entities;

namespace AnalyticsService.Application.Interfaces;

public interface IRevenueMetricRepository
{
    Task<RevenueMetricMonthly?> GetByKeyAsync(Guid propertyId, int year, int month, CancellationToken ct);
    Task<RevenueMetricMonthly?> GetByKeyForUpdateAsync(Guid propertyId, int year, int month, CancellationToken ct);
    Task<List<RevenueMetricMonthly>> GetMonthlyMetricsAsync(Guid propertyId, int year, CancellationToken ct);
    Task AddAsync(RevenueMetricMonthly metric, CancellationToken ct);
}
