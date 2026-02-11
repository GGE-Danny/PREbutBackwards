using NotificationService.Domain.Enums;

namespace NotificationService.Application.Dtos.Responses;

public record NotificationResponse(
    Guid Id,
    Guid RecipientUserId,
    NotificationType Type,
    NotificationChannel Channel,
    string Title,
    string Message,
    string? MetadataJson,
    NotificationStatus Status,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt
);
