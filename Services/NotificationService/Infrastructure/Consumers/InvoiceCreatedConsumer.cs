using System.Text.Json;
using MassTransit;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Consumers.Contracts;

namespace NotificationService.Infrastructure.Consumers;

public sealed class InvoiceCreatedConsumer : IConsumer<InvoiceCreatedEvent>
{
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<InvoiceCreatedConsumer> _logger;

    public InvoiceCreatedConsumer(
        INotificationRepository notifications,
        IUnitOfWork uow,
        ILogger<InvoiceCreatedConsumer> logger)
    {
        _notifications = notifications;
        _uow = uow;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InvoiceCreatedEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "Received InvoiceCreatedEvent: InvoiceId={InvoiceId}, TenantUserId={TenantUserId}, Amount={Amount}",
            evt.InvoiceId, evt.TenantUserId, evt.Amount);

        var metadata = JsonSerializer.Serialize(new
        {
            evt.InvoiceId,
            evt.PropertyId,
            evt.Type,
            evt.Amount,
            DueDate = evt.DueDate.ToString("yyyy-MM-dd")
        });

        var notification = new Notification
        {
            RecipientUserId = evt.TenantUserId,
            RecipientType = RecipientType.User,
            Type = NotificationType.InvoiceCreated,
            Channel = NotificationChannel.InApp,
            Title = "Invoice Created",
            Message = $"A new {evt.Type} invoice of {evt.Amount:C} has been created. Due date: {evt.DueDate:MMM dd, yyyy}.",
            MetadataJson = metadata,
            Status = NotificationStatus.Pending,
            IsRead = false
        };

        await _notifications.AddAsync(notification, context.CancellationToken);
        await _uow.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Created notification {NotificationId} for InvoiceId={InvoiceId}",
            notification.Id, evt.InvoiceId);
    }
}
