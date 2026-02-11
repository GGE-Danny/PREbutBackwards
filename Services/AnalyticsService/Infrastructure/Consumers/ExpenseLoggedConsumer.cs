using MassTransit;
using AnalyticsService.Application.Interfaces;
using AnalyticsService.Domain.Entities;
using AnalyticsService.Infrastructure.Consumers.Contracts;

namespace AnalyticsService.Infrastructure.Consumers;

public sealed class ExpenseLoggedConsumer : IConsumer<ExpenseLoggedEvent>
{
    private readonly IProcessedEventRepository _processedEvents;
    private readonly IRevenueMetricRepository _revenueMetrics;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ExpenseLoggedConsumer> _logger;

    public ExpenseLoggedConsumer(
        IProcessedEventRepository processedEvents,
        IRevenueMetricRepository revenueMetrics,
        IUnitOfWork uow,
        ILogger<ExpenseLoggedConsumer> logger)
    {
        _processedEvents = processedEvents;
        _revenueMetrics = revenueMetrics;
        _uow = uow;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ExpenseLoggedEvent> context)
    {
        var evt = context.Message;
        var messageId = context.MessageId?.ToString() ?? $"expense-{evt.ExpenseId}";

        _logger.LogInformation(
            "Received ExpenseLoggedEvent: ExpenseId={ExpenseId}, PropertyId={PropertyId}, Amount={Amount}",
            evt.ExpenseId, evt.PropertyId, evt.Amount);

        // Idempotency check
        if (await _processedEvents.ExistsAsync(messageId, context.CancellationToken))
        {
            _logger.LogInformation("Event {MessageId} already processed, skipping", messageId);
            return;
        }

        await _uow.BeginTransactionAsync(context.CancellationToken);

        try
        {
            var year = evt.IncurredAt.Year;
            var month = evt.IncurredAt.Month;

            var revenueMetric = await _revenueMetrics.GetByKeyForUpdateAsync(
                evt.PropertyId, year, month, context.CancellationToken);

            if (revenueMetric is null)
            {
                revenueMetric = new RevenueMetricMonthly
                {
                    PropertyId = evt.PropertyId,
                    Year = year,
                    Month = month,
                    RentCollected = 0m,
                    Expenses = evt.Amount,
                    NetRevenue = -evt.Amount
                };
                await _revenueMetrics.AddAsync(revenueMetric, context.CancellationToken);
            }
            else
            {
                revenueMetric.Expenses += evt.Amount;
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

            _logger.LogInformation("Processed ExpenseLoggedEvent {ExpenseId}", evt.ExpenseId);
        }
        catch
        {
            await _uow.RollbackAsync(context.CancellationToken);
            throw;
        }
    }
}
