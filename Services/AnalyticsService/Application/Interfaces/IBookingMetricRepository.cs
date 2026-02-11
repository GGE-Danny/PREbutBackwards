using AnalyticsService.Domain.Entities;

namespace AnalyticsService.Application.Interfaces;

public interface IBookingMetricRepository
{
    Task<BookingMetricDaily?> GetByKeyAsync(Guid propertyId, Guid? unitId, DateOnly date, CancellationToken ct);
    Task<BookingMetricDaily?> GetByKeyForUpdateAsync(Guid propertyId, Guid? unitId, DateOnly date, CancellationToken ct);
    Task<List<BookingMetricDaily>> GetDailyMetricsAsync(Guid propertyId, DateOnly from, DateOnly to, CancellationToken ct);
    Task<List<(Guid PropertyId, int TotalBookings)>> GetTopPropertiesAsync(DateOnly from, DateOnly to, int take, CancellationToken ct);
    Task AddAsync(BookingMetricDaily metric, CancellationToken ct);
}
