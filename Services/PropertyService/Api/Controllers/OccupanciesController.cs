using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyService.Application.Abstractions;
using PropertyService.Application.DTOs;
using PropertyService.Domain.Entities;
using PropertyService.Domain.Enums;
using PropertyService.Infrastructure.Persistence;
using PropertyService.Infrastructure.Clients;




namespace PropertyService.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class OccupanciesController : ControllerBase
{
    private readonly PropertyDbContext _db;
    private readonly ITenantProvider _tenant;
    private readonly ITenantServiceClient _tenantClient;

    public OccupanciesController(PropertyDbContext db, ITenantProvider tenant, ITenantServiceClient tenantClient)
    {
        _db = db;
        _tenant = tenant;
        _tenantClient = tenantClient;
    }

    private bool TryGetCallerUserId(out Guid userId)
    {
        userId = default;
        return Guid.TryParse(_tenant.GetUserId(), out userId);
    }

    // POST /api/v1/units/{unitId}/occupancies  (assign tenant)
    [HttpPost("units/{unitId:guid}/occupancies")]
    [Authorize(Policy = "occupancy.assign")]
    public async Task<ActionResult<OccupancyResponse>> AssignTenant(Guid unitId, [FromBody] AssignTenantRequest req)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        // Ensure unit exists and enforce owner access if owner role
        var unitQuery = _db.Units
            .Include(u => u.Property)
            .Where(u => u.Id == unitId && u.DeletedAt == null);

        var unit = await unitQuery.FirstOrDefaultAsync();
        if (unit is null) return NotFound("Unit not found.");

        if (User.IsInRole("owner") && unit.Property.OwnerId != callerUserId)
            return Forbid();

        // Close any existing current occupancy
        var current = await _db.UnitOccupancies
            .Where(o => o.UnitId == unitId && o.EndDate == null && o.DeletedAt == null)
            .OrderByDescending(o => o.StartDate)
            .FirstOrDefaultAsync();

        if (current != null)
        {
            // end it the day before the new start date (or same day if you prefer)
            var endDate = req.StartDate.AddDays(-1);
            current.EndDate = endDate;
            current.UpdatedAt = DateTime.UtcNow;
        }

        var occupancy = new UnitOccupancy
        {
            UnitId = unitId,
            TenantUserId = req.TenantUserId,
            StartDate = req.StartDate,
            EndDate = null,
            Notes = req.Notes
        };

        _db.UnitOccupancies.Add(occupancy);

        // Update unit status
        unit.Status = UnitStatus.Occupied;
        unit.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        try
        {
            await _tenantClient.NotifyOccupancyAssignedAsync(
                tenantUserId: occupancy.TenantUserId,
                propertyId: unit.PropertyId,
                unitId: unitId,
                startDate: occupancy.StartDate
            );
        }
        catch { }

        return Ok(ToResponse(occupancy));
    }

    // GET /api/v1/units/{unitId}/occupancies (history)
    [HttpGet("units/{unitId:guid}/occupancies")]
    [Authorize(Policy = "occupancy.read")]
    public async Task<ActionResult<List<OccupancyResponse>>> History(Guid unitId)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        // enforce owner access if owner role
        var unit = await _db.Units.Include(u => u.Property)
            .FirstOrDefaultAsync(u => u.Id == unitId && u.DeletedAt == null);

        if (unit is null) return NotFound("Unit not found.");
        if (User.IsInRole("owner") && unit.Property.OwnerId != callerUserId)
            return Forbid();

        var occ = await _db.UnitOccupancies.AsNoTracking()
            .Where(o => o.UnitId == unitId && o.DeletedAt == null)
            .OrderByDescending(o => o.StartDate)
            .Select(o => ToResponse(o))
            .ToListAsync();

        return Ok(occ);
    }

    // GET /api/v1/units/{unitId}/occupancies/current
    [HttpGet("units/{unitId:guid}/occupancies/current")]
    [Authorize(Policy = "occupancy.read")]
    public async Task<ActionResult<OccupancyResponse>> Current(Guid unitId)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        var unit = await _db.Units.Include(u => u.Property)
            .FirstOrDefaultAsync(u => u.Id == unitId && u.DeletedAt == null);

        if (unit is null) return NotFound("Unit not found.");
        if (User.IsInRole("owner") && unit.Property.OwnerId != callerUserId)
            return Forbid();

        var current = await _db.UnitOccupancies.AsNoTracking()
            .Where(o => o.UnitId == unitId && o.EndDate == null && o.DeletedAt == null)
            .OrderByDescending(o => o.StartDate)
            .FirstOrDefaultAsync();

        if (current is null) return NotFound("No current tenant.");

        return Ok(ToResponse(current));
    }

    // POST /api/v1/units/{unitId}/occupancies/vacate  
    public record VacateRequest(DateOnly EndDate, string? Notes);

    [HttpPost("units/{unitId:guid}/occupancies/vacate")]
    [Authorize(Policy = "occupancy.assign")]
    public async Task<IActionResult> Vacate(Guid unitId, [FromBody] VacateRequest req)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        var unit = await _db.Units
            .Include(u => u.Property)
            .FirstOrDefaultAsync(u => u.Id == unitId && u.DeletedAt == null);

        if (unit is null) return NotFound("Unit not found.");

        if (User.IsInRole("owner") && unit.Property.OwnerId != callerUserId)
            return Forbid();

        var current = await _db.UnitOccupancies
            .Where(o => o.UnitId == unitId && o.DeletedAt == null && o.EndDate == null)
            .OrderByDescending(o => o.StartDate)
            .FirstOrDefaultAsync();

        if (current is null) return NotFound("No current tenant to vacate.");

        if (req.EndDate < current.StartDate)
            return BadRequest("EndDate cannot be before StartDate.");

        current.EndDate = req.EndDate;
        current.Notes = req.Notes ?? current.Notes;
        current.UpdatedAt = DateTime.UtcNow;

        unit.Status = UnitStatus.Vacant;
        unit.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        try
        {
            await _tenantClient.NotifyOccupancyVacatedAsync(
                tenantUserId: current.TenantUserId,
                propertyId: unit.PropertyId,
                unitId: unitId,
                endDate: current.EndDate!.Value
            );
        }
        catch { }

        return NoContent();
    }

    private static OccupancyResponse ToResponse(UnitOccupancy o) => new(
        o.Id, o.UnitId, o.TenantUserId, o.StartDate, o.EndDate, o.Notes, o.CreatedAt
    );
}
