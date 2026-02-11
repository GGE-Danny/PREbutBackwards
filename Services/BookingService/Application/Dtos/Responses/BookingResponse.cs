using BookingService.Domain.Enums;

namespace BookingService.Application.Dtos.Responses;

public sealed record BookingResponse(
    Guid Id,
    Guid TenantUserId,
    Guid PropertyId,
    Guid? UnitId,
    DateOnly StartDate,
    DateOnly EndDate,
    BookingStatus Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
