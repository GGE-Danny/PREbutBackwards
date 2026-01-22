namespace PropertyService.Application.DTOs;

public record OccupancyResponse(
    Guid Id,
    Guid UnitId,
    Guid TenantUserId,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Notes,
    DateTime CreatedAt
);
