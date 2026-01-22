namespace TenantService.Application.DTOs;

public record OccupancyAssignedEvent(
    Guid TenantUserId,
    Guid PropertyId,
    Guid UnitId,
    DateOnly MoveInDate,
    string? Notes
);

public record OccupancyVacatedEvent(
    Guid TenantUserId,
    Guid PropertyId,
    Guid UnitId,
    DateOnly MoveOutDate,
    string? Notes
);
