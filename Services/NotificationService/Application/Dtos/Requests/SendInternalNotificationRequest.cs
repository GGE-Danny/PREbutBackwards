using NotificationService.Domain.Enums;

namespace NotificationService.Application.Dtos.Requests;

public record SendInternalNotificationRequest(
    Guid RecipientUserId,
    NotificationType Type,
    string Title,
    string Message,
    string? MetadataJson,
    NotificationChannel Channel = NotificationChannel.InApp
);
