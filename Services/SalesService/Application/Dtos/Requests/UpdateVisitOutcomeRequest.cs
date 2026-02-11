using SalesService.Domain.Enums;

namespace SalesService.Application.Dtos.Requests;

public record UpdateVisitOutcomeRequest(
    VisitOutcome Outcome,
    string? Notes
);
