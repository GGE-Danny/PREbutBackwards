using NotificationService.Domain.Common;
using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid RecipientUserId { get; set; }
    public RecipientType RecipientType { get; set; } = RecipientType.User;
    public NotificationType Type { get; set; }
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? MetadataJson { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? FailedReason { get; set; }
}
