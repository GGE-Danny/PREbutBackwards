namespace SalesService.Application.Dtos.Requests;

public record ScheduleVisitRequest(
    Guid LeadId,
    Guid PropertyId,
    Guid? UnitId,
    DateTime ScheduledAtUtc,
    string? Notes
);
