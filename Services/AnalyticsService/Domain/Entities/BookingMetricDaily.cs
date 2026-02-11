using AnalyticsService.Domain.Common;

namespace AnalyticsService.Domain.Entities;

/// <summary>
/// Daily booking metrics aggregated by property/unit.
/// </summary>
public class BookingMetricDaily : BaseEntity
{
    public Guid PropertyId { get; set; }
    public Guid? UnitId { get; set; }
    public DateOnly Date { get; set; }
    public int TotalBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CancelledBookings { get; set; }
}
