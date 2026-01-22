namespace PropertyService.Application.DTOs;

public record AssignTenantRequest(
    Guid TenantUserId,
    DateOnly StartDate,
    string? Notes
);
