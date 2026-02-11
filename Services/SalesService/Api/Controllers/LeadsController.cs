using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesService.Application.Dtos.Requests;
using SalesService.Application.Dtos.Responses;
using SalesService.Application.Interfaces;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Api.Controllers;

[Route("api/v1/leads")]
public sealed class LeadsController : ApiControllerBase
{
    private readonly ILeadRepository _leads;
    private readonly IUnitOfWork _uow;

    public LeadsController(ILeadRepository leads, IUnitOfWork uow)
    {
        _leads = leads;
        _uow = uow;
    }

    /// <summary>
    /// Create a new lead.
    /// Tenant can create for self; staff can create for anonymous or any tenant.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "sales.write")]
    public async Task<ActionResult<LeadResponse>> Create(
        [FromBody] CreateLeadRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var actorId))
            return Unauthorized();

        Guid? tenantUserId = null;

        if (IsTenant(User))
        {
            // Tenant creates lead for self
            tenantUserId = actorId;
        }
        // else: staff can create anonymous lead (tenantUserId remains null)

        var lead = new Lead
        {
            TenantUserId = tenantUserId,
            FullName = req.FullName,
            PhoneNumber = req.PhoneNumber,
            Email = req.Email,
            Source = req.Source,
            PropertyId = req.PropertyId,
            UnitId = req.UnitId,
            Notes = req.Notes,
            Status = LeadStatus.New,
            CreatedByUserId = actorId
        };

        await _leads.AddAsync(lead, ct);
        await _uow.SaveChangesAsync(ct);

        return Ok(ToResponse(lead));
    }

    /// <summary>
    /// Update lead status (staff only).
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "sales.write")]
    public async Task<ActionResult<LeadResponse>> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateLeadStatusRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var lead = await _leads.GetByIdForUpdateAsync(id, ct);
        if (lead is null)
            return NotFound();

        lead.Status = req.Status;
        if (!string.IsNullOrWhiteSpace(req.Notes))
            lead.Notes = req.Notes;
        lead.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        return Ok(ToResponse(lead));
    }

    /// <summary>
    /// Assign lead to a sales agent (sales.manage only).
    /// </summary>
    [HttpPatch("{id:guid}/assign")]
    [Authorize(Policy = "sales.manage")]
    public async Task<ActionResult<LeadResponse>> Assign(
        [FromRoute] Guid id,
        [FromBody] AssignLeadRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var lead = await _leads.GetByIdForUpdateAsync(id, ct);
        if (lead is null)
            return NotFound();

        lead.AssignedToUserId = req.AssignedToUserId;
        lead.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        return Ok(ToResponse(lead));
    }

    /// <summary>
    /// Get lead by ID.
    /// Tenant can only access leads where TenantUserId matches.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "sales.read")]
    public async Task<ActionResult<LeadResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        var lead = await _leads.GetByIdAsync(id, ct);
        if (lead is null)
            return NotFound();

        // Tenant-scoped access
        if (lead.TenantUserId.HasValue && !CanAccessTenant(lead.TenantUserId.Value))
            return Forbid();

        // Anonymous leads require staff access
        if (!lead.TenantUserId.HasValue && !CanManage(User))
            return Forbid();

        return Ok(ToResponse(lead));
    }

    /// <summary>
    /// Get all leads for a property (staff only).
    /// </summary>
    [HttpGet("~/api/v1/properties/{propertyId:guid}/leads")]
    [Authorize(Policy = "sales.read")]
    public async Task<ActionResult<List<LeadResponse>>> GetByProperty(
        [FromRoute] Guid propertyId,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var leads = await _leads.GetByPropertyAsync(propertyId, ct);
        return Ok(leads.Select(ToResponse).ToList());
    }

    /// <summary>
    /// Soft delete a lead (sales.manage only).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "sales.manage")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var lead = await _leads.GetByIdForUpdateAsync(id, ct);
        if (lead is null)
            return NotFound();

        lead.DeletedAt = DateTime.UtcNow;
        lead.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        return NoContent();
    }

    private static LeadResponse ToResponse(Lead l) => new(
        l.Id,
        l.TenantUserId,
        l.FullName,
        l.PhoneNumber,
        l.Email,
        l.Source,
        l.PropertyId,
        l.UnitId,
        l.Status,
        l.AssignedToUserId,
        l.Notes,
        l.CreatedAt,
        l.UpdatedAt
    );
}
