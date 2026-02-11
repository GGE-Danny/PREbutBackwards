namespace AccountingService.Application.Dtos.Requests;

public sealed record GenerateDisbursementRequest(
    Guid OwnerId,
    Guid PropertyId,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal? CommissionPercent
);
