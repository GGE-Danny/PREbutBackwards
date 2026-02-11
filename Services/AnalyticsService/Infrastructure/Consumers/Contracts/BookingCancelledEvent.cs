namespace AnalyticsService.Infrastructure.Consumers.Contracts;

public record BookingCancelledEvent
{
    public Guid BookingId { get; init; }
    public Guid PropertyId { get; init; }
    public Guid? UnitId { get; init; }
    public DateTime CancelledAt { get; init; }
}
