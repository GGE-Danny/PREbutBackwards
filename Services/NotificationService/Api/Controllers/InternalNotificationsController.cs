using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dtos.Requests;
using NotificationService.Application.Dtos.Responses;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Api.Controllers;

[Route("api/v1/internal/notifications")]
public sealed class InternalNotificationsController : ApiControllerBase
{
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _uow;

    public InternalNotificationsController(INotificationRepository notifications, IUnitOfWork uow)
    {
        _notifications = notifications;
        _uow = uow;
    }

    /// <summary>
    /// Create a notification via internal service-to-service call.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "notification.internal.write")]
    public async Task<ActionResult<NotificationResponse>> Create(
        [FromBody] SendInternalNotificationRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        var notification = new Notification
        {
            RecipientUserId = req.RecipientUserId,
            RecipientType = RecipientType.User,
            Type = req.Type,
            Channel = req.Channel,
            Title = req.Title,
            Message = req.Message,
            MetadataJson = req.MetadataJson,
            Status = NotificationStatus.Pending,
            IsRead = false
        };

        await _notifications.AddAsync(notification, ct);
        await _uow.SaveChangesAsync(ct);

        return Ok(ToResponse(notification));
    }

    private static NotificationResponse ToResponse(Notification n) => new(
        n.Id,
        n.RecipientUserId,
        n.Type,
        n.Channel,
        n.Title,
        n.Message,
        n.MetadataJson,
        n.Status,
        n.IsRead,
        n.ReadAt,
        n.CreatedAt
    );
}
