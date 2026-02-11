namespace AccountingService.Application.Dtos.Requests;

public sealed record UpdateUnitRateRequest(
    decimal Rate,
    DateOnly EffectiveFrom
);
