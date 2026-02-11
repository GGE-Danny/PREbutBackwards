namespace SalesService.Application.Dtos.Requests;

public record CreateCommissionRecordRequest(
    Guid OwnerId,
    Guid PropertyId,
    Guid? UnitId,
    Guid? BookingId,
    Guid? LeadId,
    decimal Amount,
    decimal CommissionPercent,
    DateTime EarnedAtUtc,
    string? Notes
);
