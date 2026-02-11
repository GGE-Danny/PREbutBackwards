namespace AccountingService.Infrastructure.Consumers.Contracts;

/// <summary>
/// Published when a booking transitions to Confirmed status.
/// MUST match AccountingService consumer's message type identity exactly.
/// </summary>
public record BookingConfirmedEvent
{
    public Guid BookingId { get; init; }
    public Guid TenantUserId { get; init; }
    public Guid PropertyId { get; init; }
    public Guid UnitId { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
}
