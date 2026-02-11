using AnalyticsService.Domain.Entities;

namespace AnalyticsService.Application.Interfaces;

public interface IVacancyMetricRepository
{
    Task<VacancyMetricMonthly?> GetByKeyAsync(Guid propertyId, Guid unitId, int year, int month, CancellationToken ct);
    Task<VacancyMetricMonthly?> GetByKeyForUpdateAsync(Guid propertyId, Guid unitId, int year, int month, CancellationToken ct);
    Task<List<VacancyMetricMonthly>> GetMonthlyMetricsAsync(Guid propertyId, int year, CancellationToken ct);
    Task AddAsync(VacancyMetricMonthly metric, CancellationToken ct);
}
