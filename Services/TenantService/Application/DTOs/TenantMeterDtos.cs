namespace TenantService.Application.DTOs;

public record AssignTenantMeterRequest(
    Guid PropertyId,
    Guid UnitId,
    string MeterNumber,
    string? PoleNumber,
    string? Provider,
    DateOnly StartDate,
    string? Notes
);

public record EndTenantMeterRequest(
    DateOnly EndDate,
    string? Notes
);

public record TenantMeterResponse(
    Guid Id,
    Guid TenantUserId,
    Guid PropertyId,
    Guid UnitId,
    string MeterNumber,
    string? PoleNumber,
    string? Provider,
    DateOnly StartDate,
    DateOnly? EndDate,
    DateTime CreatedAt
);
