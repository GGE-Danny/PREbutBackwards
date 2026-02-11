namespace AccountingService.Infrastructure.Consumers.Contracts;

/// <summary>
/// Event published by BookingService when a booking is confirmed.
/// Does NOT include amount - AccountingService owns pricing via UnitRate.
/// </summary>
public sealed record BookingConfirmedEvent(
    Guid BookingId,
    Guid TenantUserId,
    Guid PropertyId,
    Guid UnitId,
    DateOnly StartDate,
    DateOnly EndDate
);
