using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TenantService.Application.DTOs;
using TenantService.Domain.Entities;
using TenantService.Infrastructure.Persistence;

namespace TenantService.Api.Controllers;

[ApiController]
[Route("api/v1/tenants")]
public class TenantMetersController : ControllerBase
{
    private readonly TenantDbContext _db;

    public TenantMetersController(TenantDbContext db) => _db = db;

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

        // Optional but recommended: also prevent assigning meter for another tenant’s unit without staff access
        // (already covered above)

        // End any active meter for this unit + tenant
        var active = await _db.TenantMeterAssociations
            .Where(x => x.TenantUserId == tenantUserId && x.UnitId == req.UnitId && x.EndDate == null)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefaultAsync();

        if (active != null)
        {
            var endDate = req.StartDate.AddDays(-1);
            if (endDate < active.StartDate) endDate = req.StartDate; // safeguard

            active.EndDate = endDate;
            active.UpdatedAt = DateTime.UtcNow;
        }

        var row = new TenantMeterAssociation
        {
            TenantUserId = tenantUserId,
            PropertyId = req.PropertyId,
            UnitId = req.UnitId,
            MeterNumber = req.MeterNumber,
            PoleNumber = req.PoleNumber,
            Provider = req.Provider,
            StartDate = req.StartDate,
            EndDate = null
        };

        _db.TenantMeterAssociations.Add(row);
        await _db.SaveChangesAsync();

        return Ok(ToResponse(row));
    }

    // POST /api/v1/tenants/{tenantUserId}/meters/unassign?unitId={unitId}
    [HttpPost("{tenantUserId:guid}/meters/unassign")]
    [Authorize]
    public async Task<IActionResult> Unassign(Guid tenantUserId, [FromQuery] Guid unitId, [FromBody] EndTenantMeterRequest req)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized("Invalid user id in token.");

        if (!CanAccessTenant(tenantUserId))
            return Forbid();

        var active = await _db.TenantMeterAssociations
            .Where(x => x.TenantUserId == tenantUserId && x.UnitId == unitId && x.EndDate == null)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefaultAsync();

        if (active is null) return NotFound("No active meter association found.");

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

    private static TenantMeterResponse ToResponse(TenantMeterAssociation x) => new(
        x.Id, x.TenantUserId, x.PropertyId, x.UnitId,
        x.MeterNumber, x.PoleNumber, x.Provider,
        x.StartDate, x.EndDate, x.CreatedAt
    );
}
