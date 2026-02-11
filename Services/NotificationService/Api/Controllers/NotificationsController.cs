using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dtos.Requests;
using NotificationService.Application.Dtos.Responses;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.Api.Controllers;

[Route("api/v1/notifications")]
public sealed class NotificationsController : ApiControllerBase
{
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _uow;

    public NotificationsController(INotificationRepository notifications, IUnitOfWork uow)
    {
        _notifications = notifications;
        _uow = uow;
    }

    /// <summary>
    /// Get notifications for the current user (tenant) or specified userId (staff).
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "notification.read")]
    public async Task<ActionResult<List<NotificationResponse>>> GetNotifications(
        [FromQuery] Guid? userId,
        [FromQuery] bool? isRead,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        if (!TryGetCallerUserId(out var callerId))
            return Unauthorized();

        // Determine target user
        Guid targetUserId;
        if (userId.HasValue)
        {
            // Staff can query any user; tenant can only query self
            if (!CanAccessUser(userId.Value))
                return Forbid();
            targetUserId = userId.Value;
        }
        else
        {
            // Default to caller's own notifications
            targetUserId = callerId;
        }

        // Clamp take to max 200
        take = Math.Clamp(take, 1, 200);

        var notifications = await _notifications.GetForUserAsync(
            targetUserId, isRead, null, null, take, ct);

        return Ok(notifications.Select(ToResponse).ToList());
    }

    /// <summary>
    /// Get notification by ID.
    /// Tenant can only access own notifications.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "notification.read")]
    public async Task<ActionResult<NotificationResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        var notification = await _notifications.GetByIdAsync(id, ct);
        if (notification is null)
            return NotFound();

        if (!CanAccessUser(notification.RecipientUserId))
            return Forbid();

        return Ok(ToResponse(notification));
    }

    /// <summary>
    /// Mark notification as read/unread.
    /// </summary>
    [HttpPatch("{id:guid}/read")]
    [Authorize(Policy = "notification.write")]
    public async Task<ActionResult<NotificationResponse>> MarkRead(
        [FromRoute] Guid id,
        [FromBody] MarkNotificationReadRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        var notification = await _notifications.GetByIdForUpdateAsync(id, ct);
        if (notification is null)
            return NotFound();

        if (!CanAccessUser(notification.RecipientUserId))
            return Forbid();

        notification.IsRead = req.IsRead;
        notification.ReadAt = req.IsRead ? DateTime.UtcNow : null;
        notification.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        return Ok(ToResponse(notification));
    }

    /// <summary>
    /// Soft delete a notification.
    /// Owner can delete own; staff can delete any.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "notification.write")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        var notification = await _notifications.GetByIdForUpdateAsync(id, ct);
        if (notification is null)
            return NotFound();

        if (!CanAccessUser(notification.RecipientUserId))
            return Forbid();

        notification.DeletedAt = DateTime.UtcNow;
        notification.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        return NoContent();
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
