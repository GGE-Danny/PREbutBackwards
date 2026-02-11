namespace SalesService.Infrastructure.Consumers.Contracts;

/// <summary>
/// Event published by BookingService when a booking is confirmed.
/// Note: Uses AccountingService namespace for contract identity matching.
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
