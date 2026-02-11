using SalesService.Domain.Enums;

namespace SalesService.Application.Dtos.Responses;

public record VisitResponse(
    Guid Id,
    Guid LeadId,
    Guid PropertyId,
    Guid? UnitId,
    DateTime ScheduledAtUtc,
    VisitOutcome Outcome,
    string? Notes,
    DateTime CreatedAt
);
