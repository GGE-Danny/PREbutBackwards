namespace AnalyticsService.Infrastructure.Consumers.Contracts;

public record ExpenseLoggedEvent
{
    public Guid ExpenseId { get; init; }
    public Guid PropertyId { get; init; }
    public decimal Amount { get; init; }
    public DateTime IncurredAt { get; init; }
    public string Category { get; init; } = null!;
}
