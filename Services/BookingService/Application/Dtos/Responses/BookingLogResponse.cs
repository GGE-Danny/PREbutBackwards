using BookingService.Domain.Enums;

namespace BookingService.Application.Dtos.Responses;

public sealed record BookingLogResponse(
    Guid Id,
    Guid BookingId,
    BookingLogEventType EventType,
    string? Notes,
    Guid? ActorUserId,
    DateTime CreatedAt
);
