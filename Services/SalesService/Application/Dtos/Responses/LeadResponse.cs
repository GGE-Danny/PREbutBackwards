using SalesService.Domain.Enums;

namespace SalesService.Application.Dtos.Responses;

public record LeadResponse(
    Guid Id,
    Guid? TenantUserId,
    string FullName,
    string PhoneNumber,
    string? Email,
    string Source,
    Guid PropertyId,
    Guid? UnitId,
    LeadStatus Status,
    Guid? AssignedToUserId,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
