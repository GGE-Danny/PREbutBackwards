using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyService.Application.Abstractions;
using PropertyService.Application.DTOs;
using PropertyService.Domain.Entities;
using PropertyService.Domain.Enums;
using PropertyService.Infrastructure.Persistence;

namespace PropertyService.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class UnitsController : ControllerBase
{
    private readonly PropertyDbContext _db;
    private readonly ITenantProvider _tenant;

    public UnitsController(PropertyDbContext db, ITenantProvider tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    private bool TryGetCallerUserId(out Guid userId)
    {
        userId = default;
        return Guid.TryParse(_tenant.GetUserId(), out userId);
    }

    // POST /api/v1/properties/{propertyId}/units
    [HttpPost("properties/{propertyId:guid}/units")]
    [Authorize(Policy = "unit.create")]
    public async Task<ActionResult<UnitResponse>> CreateUnit(Guid propertyId, [FromBody] CreateUnitRequest req)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        // Owners can only create units for their own properties
        var propertyQuery = _db.Properties.Where(p => p.Id == propertyId && p.DeletedAt == null);
        if (User.IsInRole("owner"))
            propertyQuery = propertyQuery.Where(p => p.OwnerId == callerUserId);

        var propertyExists = await propertyQuery.AnyAsync();
        if (!propertyExists) return NotFound("Property not found or not accessible.");

        var unit = new Unit
        {
            PropertyId = propertyId,
            UnitNumber = req.UnitNumber.Trim(),
            Floor = req.Floor,
            Bedrooms = req.Bedrooms,
            Bathrooms = req.Bathrooms,
            Status = UnitStatus.Vacant
        };

        _db.Units.Add(unit);
        await _db.SaveChangesAsync();

        return Ok(ToResponse(unit));
    }

    // GET /api/v1/properties/{propertyId}/units
    [HttpGet("properties/{propertyId:guid}/units")]
    [Authorize(Policy = "unit.read")]
    public async Task<ActionResult<List<UnitResponse>>> ListUnits(Guid propertyId)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        // Owners can only list units for their own properties
        var propertyQuery = _db.Properties.Where(p => p.Id == propertyId && p.DeletedAt == null);
        if (User.IsInRole("owner"))
            propertyQuery = propertyQuery.Where(p => p.OwnerId == callerUserId);

        var propertyExists = await propertyQuery.AnyAsync();
        if (!propertyExists) return NotFound("Property not found or not accessible.");

        var units = await _db.Units.AsNoTracking()
            .Where(u => u.PropertyId == propertyId && u.DeletedAt == null)
            .OrderBy(u => u.UnitNumber)
            .Select(u => ToResponse(u))
            .ToListAsync();

        return Ok(units);
    }


    [HttpGet("units/{unitId:guid}")]
    [Authorize(Policy = "unit.read")]
    public async Task<ActionResult<object>> GetUnit(Guid unitId)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        var unit = await _db.Units
            .AsNoTracking()
            .Include(u => u.Property)
            .FirstOrDefaultAsync(u => u.Id == unitId && u.DeletedAt == null);

        if (unit is null) return NotFound("Unit not found.");

        // Owners can only access units in their own properties
        if (User.IsInRole("owner") && unit.Property.OwnerId != callerUserId)
            return Forbid();

        return Ok(new
        {
            unit = ToResponse(unit)
            // later we can add current tenant here
        });
    }

    private static UnitResponse ToResponse(Unit u) => new(
        u.Id, u.PropertyId, u.UnitNumber, u.Status, u.Floor, u.Bedrooms, u.Bathrooms, u.CreatedAt
    );
}
