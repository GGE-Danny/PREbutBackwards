namespace AnalyticsService.Infrastructure.Consumers.Contracts;

public record BookingConfirmedEvent
{
    public Guid BookingId { get; init; }
    public Guid TenantUserId { get; init; }
    public Guid PropertyId { get; init; }
    public Guid UnitId { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public DateTime? ConfirmedAt { get; init; }
}
