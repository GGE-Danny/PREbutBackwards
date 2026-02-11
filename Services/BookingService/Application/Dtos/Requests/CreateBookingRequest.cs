namespace BookingService.Application.Dtos.Requests;

public sealed record CreateBookingRequest(
    Guid PropertyId,
    Guid? UnitId,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Notes,
    Guid? TenantUserId // optional: staff can create on behalf; tenant MUST NOT supply
);
