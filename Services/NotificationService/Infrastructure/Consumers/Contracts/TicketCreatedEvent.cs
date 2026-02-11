namespace NotificationService.Infrastructure.Consumers.Contracts;

/// <summary>
/// Event published by SupportService when a ticket is created.
/// </summary>
public record TicketCreatedEvent
{
    public Guid TicketId { get; init; }
    public Guid? TenantUserId { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string Subject { get; init; } = null!;
    public string Status { get; init; } = null!;
}
