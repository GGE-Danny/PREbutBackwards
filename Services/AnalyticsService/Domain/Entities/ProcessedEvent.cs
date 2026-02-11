using AnalyticsService.Domain.Common;

namespace AnalyticsService.Domain.Entities;

/// <summary>
/// Tracks processed event messages for idempotency.
/// </summary>
public class ProcessedEvent : BaseEntity
{
    public string MessageId { get; set; } = null!;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
