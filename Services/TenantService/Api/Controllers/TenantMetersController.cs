using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TenantService.Application.DTOs;
using TenantService.Domain.Entities;
using TenantService.Infrastructure.Clients;
using TenantService.Infrastructure.Persistence;

namespace TenantService.Api.Controllers;

[ApiController]
[Route("api/v1/tenants")]
public class TenantMetersController : ControllerBase
{
    private readonly TenantDbContext _db;

    private readonly IPropertyServiceClient _propertyClient;

    public TenantMetersController(TenantDbContext db, IPropertyServiceClient propertyClient)
    {
        _db = db;
        _propertyClient = propertyClient;
    }


    private static bool IsTenant(ClaimsPrincipal user) => user.IsInRole("tenant");

    private static bool CanManage(ClaimsPrincipal user)
        => user.IsInRole("super_admin") || user.IsInRole("manager") || user.IsInRole("support") || user.IsInRole("sales");

    private bool TryGetCallerUserId(out Guid userId)
    {
        userId = default;

        var id =
            User.FindFirstValue("sub") ??
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(id, out userId);
    }

    private bool CanAccessTenant(Guid tenantUserId)
    {
        if (!TryGetCallerUserId(out var callerUserId)) return false;

        if (IsTenant(User))
            return tenantUserId == callerUserId;

        return CanManage(User);
    }

    // POST /api/v1/tenants/{tenantUserId}/meters/assign
    [HttpPost("{tenantUserId:guid}/meters/assign")]
    [Authorize]
    public async Task<ActionResult<TenantMeterResponse>> Assign(Guid tenantUserId, [FromBody] AssignTenantMeterRequest req)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized("Invalid user id in token.");

        if (!CanAccessTenant(tenantUserId))
            return Forbid();

        // ✅ validate first (prevents corrupting history)
        var ok = await _propertyClient.UnitBelongsToPropertyAsync(req.PropertyId, req.UnitId);
        if (!ok)
            return BadRequest("Unit does not belong to the provided PropertyId (or unit not accessible).");

        // ✅ idempotency: if same meter already active for this unit+tenant, return it
        var alreadyActive = await _db.TenantMeterAssociations
            .AsNoTracking()
            .Where(x => x.UnitId == req.UnitId && x.EndDate == null && x.DeletedAt == null)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefaultAsync();

        if (alreadyActive != null &&
            alreadyActive.TenantUserId == tenantUserId &&
            alreadyActive.PropertyId == req.PropertyId &&
            alreadyActive.MeterNumber == req.MeterNumber)
        {
            return Ok(ToResponse(alreadyActive));
        }

        // ✅ close any active meter for this UNIT (not just tenant)
        var activeForUnit = await _db.TenantMeterAssociations
            .Where(x => x.UnitId == req.UnitId && x.EndDate == null && x.DeletedAt == null)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefaultAsync();

        if (activeForUnit != null)
        {
            // close on the day before new start (but don’t go before start)
            var endDate = req.StartDate.AddDays(-1);
            if (endDate < activeForUnit.StartDate)
                endDate = req.StartDate;

            activeForUnit.EndDate = endDate;
            activeForUnit.UpdatedAt = DateTime.UtcNow;
        }

        var row = new TenantMeterAssociation
        {
            TenantUserId = tenantUserId,
            PropertyId = req.PropertyId,
            UnitId = req.UnitId,
            MeterNumber = req.MeterNumber.Trim(),
            PoleNumber = req.PoleNumber?.Trim(),
            Provider = req.Provider?.Trim(),
            StartDate = req.StartDate,
            EndDate = null
        };

        _db.TenantMeterAssociations.Add(row);
        await _db.SaveChangesAsync();

        return Ok(ToResponse(row));
    }


    // POST /api/v1/tenants/{tenantUserId}/meters/unassign?unitId={unitId}
    [HttpPost("{tenantUserId:guid}/meters/{unitId:guid}/unassign")]
    [Authorize]
    public async Task<IActionResult> Unassign(Guid tenantUserId, Guid unitId, [FromBody] EndTenantMeterRequest req)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized("Invalid user id in token.");

        if (!CanAccessTenant(tenantUserId))
            return Forbid();

        var active = await _db.TenantMeterAssociations
            .Where(x => x.UnitId == unitId && x.EndDate == null && x.DeletedAt == null)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefaultAsync();

        if (active is null) return NotFound("No active meter association found for this unit.");

        // If tenant is calling, enforce they can only unassign THEIR active row
        if (IsTenant(User) && active.TenantUserId != tenantUserId)
            return Forbid("You cannot unassign a meter for another tenant.");

        if (req.EndDate < active.StartDate)
            return BadRequest("EndDate cannot be before StartDate.");

        active.EndDate = req.EndDate;
        active.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }


    // GET /api/v1/tenants/{tenantUserId}/meters
    [HttpGet("{tenantUserId:guid}/meters")]
    [Authorize]
    public async Task<ActionResult<List<TenantMeterResponse>>> List(Guid tenantUserId)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized("Invalid user id in token.");

        if (!CanAccessTenant(tenantUserId))
            return Forbid();

        var list = await _db.TenantMeterAssociations.AsNoTracking()
            .Where(x => x.TenantUserId == tenantUserId)
            .OrderByDescending(x => x.StartDate)
            .Select(x => ToResponse(x))
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("units/{unitId:guid}/meters/current")]
    [Authorize]
    public async Task<ActionResult<TenantMeterResponse>> CurrentForUnit(Guid unitId)
    {
        var row = await _db.TenantMeterAssociations.AsNoTracking()
            .Where(x => x.UnitId == unitId && x.EndDate == null && x.DeletedAt == null)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefaultAsync();

        if (row is null) return NotFound();
        return Ok(ToResponse(row));
    }


    private static TenantMeterResponse ToResponse(TenantMeterAssociation x) => new(
        x.Id, x.TenantUserId, x.PropertyId, x.UnitId,
        x.MeterNumber, x.PoleNumber, x.Provider,
        x.StartDate, x.EndDate, x.CreatedAt
    );
}
