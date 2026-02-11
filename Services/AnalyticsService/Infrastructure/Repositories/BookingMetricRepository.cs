using Microsoft.EntityFrameworkCore;
using AnalyticsService.Application.Interfaces;
using AnalyticsService.Domain.Entities;
using AnalyticsService.Infrastructure.Persistence;

namespace AnalyticsService.Infrastructure.Repositories;

public sealed class BookingMetricRepository : IBookingMetricRepository
{
    private readonly AnalyticsDbContext _db;
    public BookingMetricRepository(AnalyticsDbContext db) => _db = db;

    public Task<BookingMetricDaily?> GetByKeyAsync(Guid propertyId, Guid? unitId, DateOnly date, CancellationToken ct)
        => _db.BookingMetricsDaily
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PropertyId == propertyId && x.UnitId == unitId && x.Date == date, ct);

    public Task<BookingMetricDaily?> GetByKeyForUpdateAsync(Guid propertyId, Guid? unitId, DateOnly date, CancellationToken ct)
        => _db.BookingMetricsDaily
            .FirstOrDefaultAsync(x => x.PropertyId == propertyId && x.UnitId == unitId && x.Date == date, ct);

    public async Task<List<BookingMetricDaily>> GetDailyMetricsAsync(Guid propertyId, DateOnly from, DateOnly to, CancellationToken ct)
        => await _db.BookingMetricsDaily
            .AsNoTracking()
            .Where(x => x.PropertyId == propertyId && x.Date >= from && x.Date <= to)
            .OrderBy(x => x.Date)
            .ToListAsync(ct);

    public async Task<List<(Guid PropertyId, int TotalBookings)>> GetTopPropertiesAsync(DateOnly from, DateOnly to, int take, CancellationToken ct)
    {
        var results = await _db.BookingMetricsDaily
            .AsNoTracking()
            .Where(x => x.Date >= from && x.Date <= to)
            .GroupBy(x => x.PropertyId)
            .Select(g => new { PropertyId = g.Key, TotalBookings = g.Sum(x => x.TotalBookings) })
            .OrderByDescending(x => x.TotalBookings)
            .Take(take)
            .ToListAsync(ct);

        return results.Select(r => (r.PropertyId, r.TotalBookings)).ToList();
    }

    public async Task AddAsync(BookingMetricDaily metric, CancellationToken ct)
        => await _db.BookingMetricsDaily.AddAsync(metric, ct);
}
