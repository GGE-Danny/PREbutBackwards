using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportService.Application.Dtos.Requests;
using SupportService.Application.Dtos.Responses;
using SupportService.Application.Interfaces;
using SupportService.Domain.Entities;
using SupportService.Domain.Enums;

namespace SupportService.Api.Controllers;

[Route("api/v1/tickets")]
public sealed class TicketsController : ApiControllerBase
{
    private readonly ITicketRepository _tickets;
    private readonly ITicketMessageRepository _messages;
    private readonly ITicketActivityRepository _activities;
    private readonly IUnitOfWork _uow;

    public TicketsController(
        ITicketRepository tickets,
        ITicketMessageRepository messages,
        ITicketActivityRepository activities,
        IUnitOfWork uow)
    {
        _tickets = tickets;
        _messages = messages;
        _activities = activities;
        _uow = uow;
    }

    /// <summary>
    /// Create a new support ticket.
    /// Tenant: TenantUserId is derived from token.
    /// Staff: TenantUserId is null (walk-in).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "support.write")]
    public async Task<ActionResult<TicketResponse>> Create(
        [FromBody] CreateTicketRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var actorId))
            return Unauthorized();

        Guid? tenantUserId = null;
        if (IsTenant(User))
        {
            tenantUserId = actorId;
        }

        var ticket = new Ticket
        {
            TenantUserId = tenantUserId,
            CreatedByUserId = actorId,
            Subject = req.Subject,
            Description = req.Description,
            Category = req.Category,
            Priority = req.Priority,
            Status = TicketStatus.Open,
            PropertyId = req.PropertyId,
            BookingId = req.BookingId
        };

        await _tickets.AddAsync(ticket, ct);

        var activity = new TicketActivity
        {
            TicketId = ticket.Id,
            Event = "Created",
            Notes = $"Ticket created with priority {req.Priority}",
            ActorUserId = actorId,
            OccurredAt = DateTime.UtcNow
        };

        await _activities.AddAsync(activity, ct);
        await _uow.SaveChangesAsync(ct);

        return Ok(ToResponse(ticket));
    }

    /// <summary>
    /// Get ticket by ID.
    /// Tenant can only access own tickets.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "support.read")]
    public async Task<ActionResult<TicketResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        var ticket = await _tickets.GetByIdAsync(id, ct);
        if (ticket is null)
            return NotFound();

        // Tenant-scoped access
        if (ticket.TenantUserId.HasValue && !CanAccessTenant(ticket.TenantUserId.Value))
            return Forbid();

        // Anonymous tickets require staff access
        if (!ticket.TenantUserId.HasValue && !CanManage(User))
            return Forbid();

        return Ok(ToResponse(ticket));
    }

    /// <summary>
    /// Get all tickets for a tenant.
    /// Tenant can only query own tickets.
    /// </summary>
    [HttpGet("~/api/v1/tenants/{tenantUserId:guid}/tickets")]
    [Authorize(Policy = "support.read")]
    public async Task<ActionResult<List<TicketResponse>>> GetByTenant(
        [FromRoute] Guid tenantUserId,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (!CanAccessTenant(tenantUserId))
            return Forbid();

        var tickets = await _tickets.GetByTenantAsync(tenantUserId, ct);
        return Ok(tickets.Select(ToResponse).ToList());
    }

    /// <summary>
    /// Assign ticket to staff (support.manage only).
    /// </summary>
    [HttpPatch("{id:guid}/assign")]
    [Authorize(Policy = "support.manage")]
    public async Task<ActionResult<TicketResponse>> Assign(
        [FromRoute] Guid id,
        [FromBody] AssignTicketRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var actorId))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var ticket = await _tickets.GetByIdForUpdateAsync(id, ct);
        if (ticket is null)
            return NotFound();

        ticket.AssignedToUserId = req.AssignedToUserId;
        ticket.UpdatedAt = DateTime.UtcNow;

        var activity = new TicketActivity
        {
            TicketId = ticket.Id,
            Event = "Assigned",
            Notes = $"Assigned to {req.AssignedToUserId}",
            ActorUserId = actorId,
            OccurredAt = DateTime.UtcNow
        };

        await _activities.AddAsync(activity, ct);
        await _uow.SaveChangesAsync(ct);

        return Ok(ToResponse(ticket));
    }

    /// <summary>
    /// Update ticket status.
    /// Tenant can only set status to Closed.
    /// Staff can set any status.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "support.write")]
    public async Task<ActionResult<TicketResponse>> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateTicketStatusRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var actorId))
            return Unauthorized();

        var ticket = await _tickets.GetByIdForUpdateAsync(id, ct);
        if (ticket is null)
            return NotFound();

        // Access check
        if (ticket.TenantUserId.HasValue && !CanAccessTenant(ticket.TenantUserId.Value))
            return Forbid();

        if (!ticket.TenantUserId.HasValue && !CanManage(User))
            return Forbid();

        // Tenant can only close tickets
        if (IsTenant(User) && req.Status != TicketStatus.Closed)
            return BadRequest("Tenants can only close tickets.");

        var oldStatus = ticket.Status;
        ticket.Status = req.Status;
        ticket.UpdatedAt = DateTime.UtcNow;

        var activity = new TicketActivity
        {
            TicketId = ticket.Id,
            Event = "StatusChanged",
            Notes = $"Status changed from {oldStatus} to {req.Status}" +
                    (string.IsNullOrWhiteSpace(req.Notes) ? "" : $". {req.Notes}"),
            ActorUserId = actorId,
            OccurredAt = DateTime.UtcNow
        };

        await _activities.AddAsync(activity, ct);
        await _uow.SaveChangesAsync(ct);

        return Ok(ToResponse(ticket));
    }

    /// <summary>
    /// Add a message to the ticket.
    /// Tenant can only message own tickets.
    /// </summary>
    [HttpPost("{id:guid}/messages")]
    [Authorize(Policy = "support.write")]
    public async Task<ActionResult<TicketMessageResponse>> AddMessage(
        [FromRoute] Guid id,
        [FromBody] AddTicketMessageRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var actorId))
            return Unauthorized();

        var ticket = await _tickets.GetByIdForUpdateAsync(id, ct);
        if (ticket is null)
            return NotFound();

        // Access check
        if (ticket.TenantUserId.HasValue && !CanAccessTenant(ticket.TenantUserId.Value))
            return Forbid();

        if (!ticket.TenantUserId.HasValue && !CanManage(User))
            return Forbid();

        var messageType = IsTenant(User) ? TicketMessageType.Customer : TicketMessageType.Staff;

        var message = new TicketMessage
        {
            TicketId = ticket.Id,
            MessageType = messageType,
            Body = req.Body,
            ActorUserId = actorId
        };

        await _messages.AddAsync(message, ct);

        var activity = new TicketActivity
        {
            TicketId = ticket.Id,
            Event = "Commented",
            Notes = $"{messageType} added a message",
            ActorUserId = actorId,
            OccurredAt = DateTime.UtcNow
        };

        await _activities.AddAsync(activity, ct);

        ticket.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);

        return Ok(ToMessageResponse(message));
    }

    /// <summary>
    /// Get all messages for a ticket.
    /// </summary>
    [HttpGet("{id:guid}/messages")]
    [Authorize(Policy = "support.read")]
    public async Task<ActionResult<List<TicketMessageResponse>>> GetMessages(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        var ticket = await _tickets.GetByIdAsync(id, ct);
        if (ticket is null)
            return NotFound();

        // Access check
        if (ticket.TenantUserId.HasValue && !CanAccessTenant(ticket.TenantUserId.Value))
            return Forbid();

        if (!ticket.TenantUserId.HasValue && !CanManage(User))
            return Forbid();

        var messages = await _messages.GetByTicketAsync(id, ct);
        return Ok(messages.Select(ToMessageResponse).ToList());
    }

    /// <summary>
    /// Get all activities for a ticket.
    /// </summary>
    [HttpGet("{id:guid}/activities")]
    [Authorize(Policy = "support.read")]
    public async Task<ActionResult<List<TicketActivityResponse>>> GetActivities(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        var ticket = await _tickets.GetByIdAsync(id, ct);
        if (ticket is null)
            return NotFound();

        // Access check
        if (ticket.TenantUserId.HasValue && !CanAccessTenant(ticket.TenantUserId.Value))
            return Forbid();

        if (!ticket.TenantUserId.HasValue && !CanManage(User))
            return Forbid();

        var activities = await _activities.GetByTicketAsync(id, ct);
        return Ok(activities.Select(ToActivityResponse).ToList());
    }

    /// <summary>
    /// Soft delete a ticket (support.manage only).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "support.manage")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var actorId))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var ticket = await _tickets.GetByIdForUpdateAsync(id, ct);
        if (ticket is null)
            return NotFound();

        ticket.DeletedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;

        var activity = new TicketActivity
        {
            TicketId = ticket.Id,
            Event = "Deleted",
            Notes = "Ticket soft deleted",
            ActorUserId = actorId,
            OccurredAt = DateTime.UtcNow
        };

        await _activities.AddAsync(activity, ct);
        await _uow.SaveChangesAsync(ct);

        return NoContent();
    }

    private static TicketResponse ToResponse(Ticket t) => new(
        t.Id,
        t.TenantUserId,
        t.CreatedByUserId,
        t.AssignedToUserId,
        t.Subject,
        t.Description,
        t.Category,
        t.Priority,
        t.Status,
        t.PropertyId,
        t.BookingId,
        t.CreatedAt,
        t.UpdatedAt
    );

    private static TicketMessageResponse ToMessageResponse(TicketMessage m) => new(
        m.Id,
        m.TicketId,
        m.MessageType,
        m.Body,
        m.ActorUserId,
        m.CreatedAt
    );

    private static TicketActivityResponse ToActivityResponse(TicketActivity a) => new(
        a.Id,
        a.TicketId,
        a.Event,
        a.Notes,
        a.ActorUserId,
        a.OccurredAt
    );
}
