using AccountingService.Domain.Enums;

namespace AccountingService.Application.Dtos.Requests;

public sealed record LogExpenseRequest(
    Guid PropertyId,
    decimal Amount,
    ExpenseCategory Category,
    string Description,
    DateTime IncurredAt
);
