using BookingService.Domain.Enums;

namespace BookingService.Application.Dtos.Requests;

public sealed record UpdateBookingStatusRequest(
    BookingStatus Status,
    string? Notes
);
