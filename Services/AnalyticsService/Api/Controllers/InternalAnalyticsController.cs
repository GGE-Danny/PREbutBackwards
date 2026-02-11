using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AnalyticsService.Application.Dtos.Requests;
using AnalyticsService.Application.Interfaces;
using AnalyticsService.Domain.Entities;

namespace AnalyticsService.Api.Controllers;

[Route("api/v1/internal/analytics")]
[Authorize(Policy = "RequireAdminOrSystem")]
public class InternalAnalyticsController : ApiControllerBase
{
    private readonly IProcessedEventRepository _processedEvents;
    private readonly IRevenueMetricRepository _revenueMetrics;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<InternalAnalyticsController> _logger;

    public InternalAnalyticsController(
        IProcessedEventRepository processedEvents,
        IRevenueMetricRepository revenueMetrics,
        IUnitOfWork uow,
        ILogger<InternalAnalyticsController> logger)
    {
        _processedEvents = processedEvents;
        _revenueMetrics = revenueMetrics;
        _uow = uow;
        _logger = logger;
    }

    /// <summary>
    /// Record a payment for analytics (internal endpoint for services that don't publish events).
    /// </summary>
    [HttpPost("payments")]
    public async Task<IActionResult> RecordPayment([FromBody] RecordPaymentRequest request, CancellationToken ct)
    {
        var messageId = $"payment-{request.InvoiceId}";

        // Only process Rent payments for revenue metrics
        if (!string.Equals(request.Type, "Rent", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Skipping non-Rent payment type: {Type}", request.Type);
            return Ok(new { message = "Non-rent payment skipped" });
        }

        // Idempotency check
        if (await _processedEvents.ExistsAsync(messageId, ct))
        {
            _logger.LogInformation("Payment {MessageId} already processed", messageId);
            return Ok(new { message = "Already processed" });
        }

        await _uow.BeginTransactionAsync(ct);

        try
        {
            var paidAt = DateTime.SpecifyKind(request.PaidAtUtc, DateTimeKind.Utc);
            var year = paidAt.Year;
            var month = paidAt.Month;

            var revenueMetric = await _revenueMetrics.GetByKeyForUpdateAsync(
                request.PropertyId, year, month, ct);

            if (revenueMetric is null)
            {
                revenueMetric = new RevenueMetricMonthly
                {
                    PropertyId = request.PropertyId,
                    Year = year,
                    Month = month,
                    RentCollected = request.Amount,
                    Expenses = 0m,
                    NetRevenue = request.Amount
                };
                await _revenueMetrics.AddAsync(revenueMetric, ct);
            }
            else
            {
                revenueMetric.RentCollected += request.Amount;
                revenueMetric.NetRevenue = revenueMetric.RentCollected - revenueMetric.Expenses;
                revenueMetric.UpdatedAt = DateTime.UtcNow;
            }

            await _processedEvents.AddAsync(new ProcessedEvent
            {
                MessageId = messageId,
                ProcessedAt = DateTime.UtcNow
            }, ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            _logger.LogInformation("Recorded payment {InvoiceId} for analytics", request.InvoiceId);
            return Ok(new { message = "Payment recorded" });
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// Record an expense for analytics (internal endpoint for services that don't publish events).
    /// </summary>
    [HttpPost("expenses")]
    public async Task<IActionResult> RecordExpense([FromBody] RecordExpenseRequest request, CancellationToken ct)
    {
        var messageId = $"expense-{request.ExpenseId}";

        // Idempotency check
        if (await _processedEvents.ExistsAsync(messageId, ct))
        {
            _logger.LogInformation("Expense {MessageId} already processed", messageId);
            return Ok(new { message = "Already processed" });
        }

        await _uow.BeginTransactionAsync(ct);

        try
        {
            var incurredAt = DateTime.SpecifyKind(request.IncurredAtUtc, DateTimeKind.Utc);
            var year = incurredAt.Year;
            var month = incurredAt.Month;

            var revenueMetric = await _revenueMetrics.GetByKeyForUpdateAsync(
                request.PropertyId, year, month, ct);

            if (revenueMetric is null)
            {
                revenueMetric = new RevenueMetricMonthly
                {
                    PropertyId = request.PropertyId,
                    Year = year,
                    Month = month,
                    RentCollected = 0m,
                    Expenses = request.Amount,
                    NetRevenue = -request.Amount
                };
                await _revenueMetrics.AddAsync(revenueMetric, ct);
            }
            else
            {
                revenueMetric.Expenses += request.Amount;
                revenueMetric.NetRevenue = revenueMetric.RentCollected - revenueMetric.Expenses;
                revenueMetric.UpdatedAt = DateTime.UtcNow;
            }

            await _processedEvents.AddAsync(new ProcessedEvent
            {
                MessageId = messageId,
                ProcessedAt = DateTime.UtcNow
            }, ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            _logger.LogInformation("Recorded expense {ExpenseId} for analytics", request.ExpenseId);
            return Ok(new { message = "Expense recorded" });
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
