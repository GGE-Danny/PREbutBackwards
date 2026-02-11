using Microsoft.EntityFrameworkCore;
using AnalyticsService.Application.Interfaces;
using AnalyticsService.Domain.Entities;
using AnalyticsService.Infrastructure.Persistence;

namespace AnalyticsService.Infrastructure.Repositories;

public sealed class VacancyMetricRepository : IVacancyMetricRepository
{
    private readonly AnalyticsDbContext _db;
    public VacancyMetricRepository(AnalyticsDbContext db) => _db = db;

    public Task<VacancyMetricMonthly?> GetByKeyAsync(Guid propertyId, Guid unitId, int year, int month, CancellationToken ct)
        => _db.VacancyMetricsMonthly
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PropertyId == propertyId && x.UnitId == unitId && x.Year == year && x.Month == month, ct);

    public Task<VacancyMetricMonthly?> GetByKeyForUpdateAsync(Guid propertyId, Guid unitId, int year, int month, CancellationToken ct)
        => _db.VacancyMetricsMonthly
            .FirstOrDefaultAsync(x => x.PropertyId == propertyId && x.UnitId == unitId && x.Year == year && x.Month == month, ct);

    public async Task<List<VacancyMetricMonthly>> GetMonthlyMetricsAsync(Guid propertyId, int year, CancellationToken ct)
        => await _db.VacancyMetricsMonthly
            .AsNoTracking()
            .Where(x => x.PropertyId == propertyId && x.Year == year)
            .OrderBy(x => x.Month)
            .ToListAsync(ct);

    public async Task AddAsync(VacancyMetricMonthly metric, CancellationToken ct)
        => await _db.VacancyMetricsMonthly.AddAsync(metric, ct);
}
