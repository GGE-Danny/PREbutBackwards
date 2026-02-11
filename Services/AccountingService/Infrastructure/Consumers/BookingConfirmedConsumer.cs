using AccountingService.Application.Interfaces;
using AccountingService.Domain.Entities;
using AccountingService.Domain.Enums;
using AccountingService.Infrastructure.Consumers.Contracts;
using AccountingService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AccountingService.Infrastructure.Consumers;

public sealed class BookingConfirmedConsumer : IConsumer<BookingConfirmedEvent>
{
    private readonly AccountingDbContext _db;
    private readonly IUnitRateRepository _unitRates;
    private readonly ILogger<BookingConfirmedConsumer> _logger;

    public BookingConfirmedConsumer(
        AccountingDbContext db,
        IUnitRateRepository unitRates,
        ILogger<BookingConfirmedConsumer> logger)
    {
        _db = db;
        _unitRates = unitRates;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
    {
        var msg = context.Message;
        var ct = context.CancellationToken;

        // Idempotent: one Rent invoice per booking
        var exists = await _db.Invoices
            .AnyAsync(x => x.BookingId == msg.BookingId && x.Type == InvoiceType.Rent, ct);

        if (exists) return;

        // Look up active rate for the unit
        var rate = await _unitRates.GetActiveRateForUnitAsync(msg.UnitId, msg.StartDate, ct);

        decimal amount;
        if (rate is null)
        {
            // MVP safe behavior: create invoice with 0 and log warning
            _logger.LogWarning(
                "No active UnitRate found for UnitId={UnitId}, PropertyId={PropertyId}. Creating invoice with Amount=0.",
                msg.UnitId, msg.PropertyId);
            amount = 0m;
        }
        else
        {
            // Calculate amount: Rate is monthly, dates are inclusive
            // Days = EndDate - StartDate + 1
            var days = msg.EndDate.DayNumber - msg.StartDate.DayNumber + 1;
            // Daily proration: Rate / 30 * days
            amount = Math.Round(rate.Rate / 30m * days, 2);
        }

        // DueDate: StartDate of the booking (tenant should pay before moving in)
        var invoice = new Invoice(
            bookingId: msg.BookingId,
            tenantUserId: msg.TenantUserId,
            propertyId: msg.PropertyId,
            amount: amount,
            dueDate: msg.StartDate,
            status: PaymentStatus.Unpaid,
            type: InvoiceType.Rent
        );

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created Rent invoice for BookingId={BookingId}, Amount={Amount}, DueDate={DueDate}",
            msg.BookingId, amount, msg.StartDate);
    }
}
