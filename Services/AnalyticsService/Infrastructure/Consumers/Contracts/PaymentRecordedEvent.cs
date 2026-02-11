namespace AnalyticsService.Infrastructure.Consumers.Contracts;

public record PaymentRecordedEvent
{
    public Guid InvoiceId { get; init; }
    public Guid PropertyId { get; init; }
    public Guid TenantUserId { get; init; }
    public decimal Amount { get; init; }
    public DateTime PaidAt { get; init; }
    public string Type { get; init; } = null!; // "Rent" or "ServiceFee"
}
