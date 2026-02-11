using Microsoft.EntityFrameworkCore;
using AnalyticsService.Application.Interfaces;
using AnalyticsService.Domain.Entities;
using AnalyticsService.Infrastructure.Persistence;

namespace AnalyticsService.Infrastructure.Repositories;

public sealed class RevenueMetricRepository : IRevenueMetricRepository
{
    private readonly AnalyticsDbContext _db;
    public RevenueMetricRepository(AnalyticsDbContext db) => _db = db;

    public Task<RevenueMetricMonthly?> GetByKeyAsync(Guid propertyId, int year, int month, CancellationToken ct)
        => _db.RevenueMetricsMonthly
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PropertyId == propertyId && x.Year == year && x.Month == month, ct);

    public Task<RevenueMetricMonthly?> GetByKeyForUpdateAsync(Guid propertyId, int year, int month, CancellationToken ct)
        => _db.RevenueMetricsMonthly
            .FirstOrDefaultAsync(x => x.PropertyId == propertyId && x.Year == year && x.Month == month, ct);

    public async Task<List<RevenueMetricMonthly>> GetMonthlyMetricsAsync(Guid propertyId, int year, CancellationToken ct)
        => await _db.RevenueMetricsMonthly
            .AsNoTracking()
            .Where(x => x.PropertyId == propertyId && x.Year == year)
            .OrderBy(x => x.Month)
            .ToListAsync(ct);

    public async Task AddAsync(RevenueMetricMonthly metric, CancellationToken ct)
        => await _db.RevenueMetricsMonthly.AddAsync(metric, ct);
}
