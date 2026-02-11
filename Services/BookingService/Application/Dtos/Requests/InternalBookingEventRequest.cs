namespace BookingService.Application.Dtos.Requests;

public sealed record InternalBookingEventRequest(
    Guid BookingId,
    string? Notes,
    Guid? ActorUserId
);
