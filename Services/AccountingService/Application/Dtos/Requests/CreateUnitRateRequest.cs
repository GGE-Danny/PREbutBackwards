namespace AccountingService.Application.Dtos.Requests;

public sealed record CreateUnitRateRequest(
    Guid PropertyId,
    Guid UnitId,
    decimal Rate,
    DateOnly EffectiveFrom
);
