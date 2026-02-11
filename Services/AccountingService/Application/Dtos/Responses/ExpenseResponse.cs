using AccountingService.Domain.Enums;

namespace AccountingService.Application.Dtos.Responses;

public sealed record ExpenseResponse(
    Guid Id,
    Guid PropertyId,
    decimal Amount,
    ExpenseCategory Category,
    string Description,
    DateTime IncurredAt,
    DateTime CreatedAt
);
