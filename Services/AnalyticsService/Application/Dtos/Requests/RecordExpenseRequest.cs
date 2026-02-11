namespace AnalyticsService.Application.Dtos.Requests;

/// <summary>
/// Internal endpoint request to record an expense (if AccountingService doesn't publish events yet).
/// </summary>
public record RecordExpenseRequest(
    Guid ExpenseId,
    Guid PropertyId,
    decimal Amount,
    DateTime IncurredAtUtc,
    string Category
);
