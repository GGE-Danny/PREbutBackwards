using NotificationService.Domain.Enums;

namespace NotificationService.Application.Dtos.Requests;

public record CreateNotificationRequest(
    Guid RecipientUserId,
    NotificationType Type,
    NotificationChannel Channel,
    string Title,
    string Message,
    string? MetadataJson
);
