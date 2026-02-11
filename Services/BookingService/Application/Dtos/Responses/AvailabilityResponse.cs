namespace BookingService.Application.Dtos.Responses;

public sealed record AvailabilityResponse(
    Guid PropertyId,
    Guid UnitId,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsAvailable,
    Guid? ConflictingBookingId
);
