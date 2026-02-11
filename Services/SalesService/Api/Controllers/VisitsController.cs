using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesService.Application.Dtos.Requests;
using SalesService.Application.Dtos.Responses;
using SalesService.Application.Interfaces;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Api.Controllers;

[Route("api/v1/visits")]
public sealed class VisitsController : ApiControllerBase
{
    private readonly IVisitRepository _visits;
    private readonly ILeadRepository _leads;
    private readonly IUnitOfWork _uow;

    public VisitsController(
        IVisitRepository visits,
        ILeadRepository leads,
        IUnitOfWork uow)
    {
        _visits = visits;
        _leads = leads;
        _uow = uow;
    }

    /// <summary>
    /// Schedule a visit for a lead (staff only).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "sales.write")]
    public async Task<ActionResult<VisitResponse>> Schedule(
        [FromBody] ScheduleVisitRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var actorId))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        // Validate lead exists
        var lead = await _leads.GetByIdForUpdateAsync(req.LeadId, ct);
        if (lead is null)
            return BadRequest("Lead not found.");

        // Ensure ScheduledAt is UTC
        var scheduledAtUtc = req.ScheduledAtUtc.Kind == DateTimeKind.Utc
            ? req.ScheduledAtUtc
            : DateTime.SpecifyKind(req.ScheduledAtUtc, DateTimeKind.Utc);

        var visit = new Visit
        {
            LeadId = req.LeadId,
            PropertyId = req.PropertyId,
            UnitId = req.UnitId,
            ScheduledAt = scheduledAtUtc,
            Outcome = VisitOutcome.Pending,
            Notes = req.Notes,
            ActorUserId = actorId
        };

        await _visits.AddAsync(visit, ct);

        // Update lead status to ViewingScheduled if currently New or Contacted
        if (lead.Status is LeadStatus.New or LeadStatus.Contacted)
        {
            lead.Status = LeadStatus.ViewingScheduled;
            lead.UpdatedAt = DateTime.UtcNow;
        }

        await _uow.SaveChangesAsync(ct);

        return Ok(ToResponse(visit));
    }

    /// <summary>
    /// Update visit outcome (staff only).
    /// </summary>
    [HttpPatch("{id:guid}/outcome")]
    [Authorize(Policy = "sales.write")]
    public async Task<ActionResult<VisitResponse>> UpdateOutcome(
        [FromRoute] Guid id,
        [FromBody] UpdateVisitOutcomeRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var visit = await _visits.GetByIdForUpdateAsync(id, ct);
        if (visit is null)
            return NotFound();

        visit.Outcome = req.Outcome;
        if (!string.IsNullOrWhiteSpace(req.Notes))
            visit.Notes = req.Notes;
        visit.UpdatedAt = DateTime.UtcNow;

        // Update lead status based on outcome
        var lead = await _leads.GetByIdForUpdateAsync(visit.LeadId, ct);
        if (lead is not null && lead.Status == LeadStatus.ViewingScheduled)
        {
            lead.Status = LeadStatus.ViewingDone;
            lead.UpdatedAt = DateTime.UtcNow;
        }

        await _uow.SaveChangesAsync(ct);

        return Ok(ToResponse(visit));
    }

    /// <summary>
    /// Get all visits for a lead (staff only).
    /// </summary>
    [HttpGet("~/api/v1/leads/{leadId:guid}/visits")]
    [Authorize(Policy = "sales.read")]
    public async Task<ActionResult<List<VisitResponse>>> GetByLead(
        [FromRoute] Guid leadId,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var visits = await _visits.GetByLeadAsync(leadId, ct);
        return Ok(visits.Select(ToResponse).ToList());
    }

    private static VisitResponse ToResponse(Visit v) => new(
        v.Id,
        v.LeadId,
        v.PropertyId,
        v.UnitId,
        v.ScheduledAt,
        v.Outcome,
        v.Notes,
        v.CreatedAt
    );
}
