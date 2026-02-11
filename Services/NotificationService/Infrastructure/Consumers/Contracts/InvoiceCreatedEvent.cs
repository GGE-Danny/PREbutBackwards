namespace NotificationService.Infrastructure.Consumers.Contracts;

/// <summary>
/// Event published by AccountingService when an invoice is created.
/// </summary>
public record InvoiceCreatedEvent
{
    public Guid InvoiceId { get; init; }
    public Guid TenantUserId { get; init; }
    public Guid PropertyId { get; init; }
    public decimal Amount { get; init; }
    public DateOnly DueDate { get; init; }
    public string Type { get; init; } = null!; // "Rent" or "ServiceFee"
}
