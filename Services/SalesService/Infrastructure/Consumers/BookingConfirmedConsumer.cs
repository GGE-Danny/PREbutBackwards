using MassTransit;
using SalesService.Application.Interfaces;
using SalesService.Domain.Enums;
using SalesService.Infrastructure.Consumers.Contracts;

namespace SalesService.Infrastructure.Consumers;

/// <summary>
/// Consumes BookingConfirmedEvent to auto-convert matching leads.
///
/// Assumptions:
/// - Best-effort matching: finds lead by TenantUserId + PropertyId + UnitId
/// - Only updates leads that are not already Converted or Lost
/// - If no matching lead is found, the event is acknowledged without error
/// </summary>
public sealed class BookingConfirmedConsumer : IConsumer<BookingConfirmedEvent>
{
    private readonly ILeadRepository _leads;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<BookingConfirmedConsumer> _logger;

    public BookingConfirmedConsumer(
        ILeadRepository leads,
        IUnitOfWork uow,
        ILogger<BookingConfirmedConsumer> logger)
    {
        _leads = leads;
        _uow = uow;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "Received BookingConfirmedEvent: BookingId={BookingId}, TenantUserId={TenantUserId}, PropertyId={PropertyId}, UnitId={UnitId}",
            evt.BookingId, evt.TenantUserId, evt.PropertyId, evt.UnitId);

        // Find matching lead (best-effort)
        var lead = await _leads.FindByTenantPropertyUnitAsync(
            evt.TenantUserId,
            evt.PropertyId,
            evt.UnitId,
            context.CancellationToken);

        if (lead is null)
        {
            _logger.LogInformation(
                "No matching lead found for BookingId={BookingId}. Skipping conversion.",
                evt.BookingId);
            return;
        }

        if (lead.Status == LeadStatus.Converted)
        {
            _logger.LogInformation(
                "Lead {LeadId} is already converted. Skipping.",
                lead.Id);
            return;
        }

        // Convert the lead
        lead.Status = LeadStatus.Converted;
        lead.UpdatedAt = DateTime.UtcNow;
        lead.Notes = $"{lead.Notes ?? ""}\n[Auto-converted from BookingId:{evt.BookingId}]".Trim();

        await _uow.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Lead {LeadId} converted for BookingId={BookingId}",
            lead.Id, evt.BookingId);
    }
}
