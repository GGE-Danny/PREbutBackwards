using MassTransit;
using AnalyticsService.Application.Interfaces;
using AnalyticsService.Domain.Entities;
using AnalyticsService.Infrastructure.Consumers.Contracts;

namespace AnalyticsService.Infrastructure.Consumers;

public sealed class PaymentRecordedConsumer : IConsumer<PaymentRecordedEvent>
{
    private readonly IProcessedEventRepository _processedEvents;
    private readonly IRevenueMetricRepository _revenueMetrics;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PaymentRecordedConsumer> _logger;

    public PaymentRecordedConsumer(
        IProcessedEventRepository processedEvents,
        IRevenueMetricRepository revenueMetrics,
        IUnitOfWork uow,
        ILogger<PaymentRecordedConsumer> logger)
    {
        _processedEvents = processedEvents;
        _revenueMetrics = revenueMetrics;
        _uow = uow;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentRecordedEvent> context)
    {
        var evt = context.Message;
        var messageId = context.MessageId?.ToString() ?? $"payment-{evt.InvoiceId}";

        _logger.LogInformation(
            "Received PaymentRecordedEvent: InvoiceId={InvoiceId}, PropertyId={PropertyId}, Amount={Amount}",
            evt.InvoiceId, evt.PropertyId, evt.Amount);

        // Only process Rent payments for revenue metrics
        if (!string.Equals(evt.Type, "Rent", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Skipping non-Rent payment type: {Type}", evt.Type);
            return;
        }

        // Idempotency check
        if (await _processedEvents.ExistsAsync(messageId, context.CancellationToken))
        {
            _logger.LogInformation("Event {MessageId} already processed, skipping", messageId);
            return;
        }

        await _uow.BeginTransactionAsync(context.CancellationToken);

        try
        {
            var year = evt.PaidAt.Year;
            var month = evt.PaidAt.Month;

            var revenueMetric = await _revenueMetrics.GetByKeyForUpdateAsync(
                evt.PropertyId, year, month, context.CancellationToken);

            if (revenueMetric is null)
            {
                revenueMetric = new RevenueMetricMonthly
                {
                    PropertyId = evt.PropertyId,
                    Year = year,
                    Month = month,
                    RentCollected = evt.Amount,
                    Expenses = 0m,
                    NetRevenue = evt.Amount
                };
                await _revenueMetrics.AddAsync(revenueMetric, context.CancellationToken);
            }
            else
            {
                revenueMetric.RentCollected += evt.Amount;
                revenueMetric.NetRevenue = revenueMetric.RentCollected - revenueMetric.Expenses;
                revenueMetric.UpdatedAt = DateTime.UtcNow;
            }

            await _processedEvents.AddAsync(new ProcessedEvent
            {
                MessageId = messageId,
                ProcessedAt = DateTime.UtcNow
            }, context.CancellationToken);

            await _uow.SaveChangesAsync(context.CancellationToken);
            await _uow.CommitAsync(context.CancellationToken);

            _logger.LogInformation("Processed PaymentRecordedEvent {InvoiceId}", evt.InvoiceId);
        }
        catch
        {
            await _uow.RollbackAsync(context.CancellationToken);
            throw;
        }
    }
}
