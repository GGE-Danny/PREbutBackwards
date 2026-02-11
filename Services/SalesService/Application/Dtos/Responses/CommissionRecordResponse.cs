using SalesService.Domain.Enums;

namespace SalesService.Application.Dtos.Responses;

public record CommissionRecordResponse(
    Guid Id,
    Guid OwnerId,
    Guid PropertyId,
    Guid? UnitId,
    Guid? BookingId,
    Guid? LeadId,
    decimal Amount,
    decimal CommissionPercent,
    CommissionStatus Status,
    DateTime EarnedAtUtc,
    string? Notes,
    DateTime CreatedAt
);
