namespace AccountingService.Application.Dtos.Responses;

public sealed record OwnerDisbursementResponse(
    Guid Id,
    Guid OwnerId,
    Guid PropertyId,
    decimal Amount,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    bool IsPaid,
    DateTime CreatedAt
);
