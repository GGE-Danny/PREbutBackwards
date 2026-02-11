namespace AccountingService.Application.Dtos.Responses;

public sealed record UnitRateResponse(
    Guid Id,
    Guid PropertyId,
    Guid UnitId,
    decimal Rate,
    bool IsActive,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    DateTime CreatedAt
);
