namespace TenantService.Application.DTOs;

public record TenantResidencyResponse(
    Guid Id,
    Guid TenantUserId,
    Guid PropertyId,
    Guid UnitId,
    DateOnly MoveInDate,
    DateOnly? MoveOutDate,
    string Source,
    string? Notes,
    DateTime CreatedAt
);
