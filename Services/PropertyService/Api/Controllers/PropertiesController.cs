using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyService.Application.Abstractions;
using PropertyService.Application.DTOs;
using PropertyService.Domain.Entities;
using PropertyService.Domain.Enums;
using PropertyService.Infrastructure.Persistence;
using PropertyService.Infrastructure.Services;

namespace PropertyService.Api.Controllers;

[ApiController]
[Route("api/v1/properties")]
public class PropertiesController : ControllerBase
{
    private readonly PropertyDbContext _db;
    private readonly ITenantProvider _tenant;
    private readonly ITimelineWriter _timeline;

    public PropertiesController(PropertyDbContext db, ITenantProvider tenant, ITimelineWriter timeline)
    {
        _db = db;
        _tenant = tenant;
        _timeline = timeline;
    }

    private bool TryGetCallerUserId(out Guid userId)
    {
        userId = default;
        var s = _tenant.GetUserId();
        return Guid.TryParse(s, out userId);
    }

    [HttpPost]
    [Authorize(Policy = "property.create")]
    public async Task<ActionResult<PropertyResponse>> Create([FromBody] CreatePropertyRequest req)
    {
        var callerUserIdStr = _tenant.GetUserId();
        if (!Guid.TryParse(callerUserIdStr, out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        Guid? ownerId;
        if (User.IsInRole("owner"))
            ownerId = callerUserId;                 // force owner to self
        else
            ownerId = req.OwnerId ?? callerUserId;  // staff can set or default

        var entity = new Property
        {
            Name = req.Name,
            Type = req.Type,
            Status = PropertyStatus.Draft,
            OwnerId = ownerId,

            AddressLine = req.AddressLine,
            Area = req.Area,
            City = req.City,
            Region = req.Region,
            Landmark = req.Landmark,
            GpsLatitude = req.GpsLatitude,
            GpsLongitude = req.GpsLongitude,
            Notes = req.Notes
        };

        _db.Properties.Add(entity);
        await _db.SaveChangesAsync();

        await _timeline.WritePropertyEventAsync(entity.Id, "PropertyCreated", new { entity.Name, entity.Type, ownerId });

        return Ok(ToResponse(entity));
    }


    [HttpGet]
    [Authorize(Policy = "property.read")]
    public async Task<ActionResult<List<PropertyResponse>>> List(
      [FromQuery] PropertyStatus? status,
      [FromQuery] PropertyType? type,
      [FromQuery] string? q)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        var query = _db.Properties.AsNoTracking()
            .Where(p => p.DeletedAt == null);

        // Owners only see their own properties
        if (User.IsInRole("owner"))
            query = query.Where(p => p.OwnerId == callerUserId);

        if (status is not null) query = query.Where(p => p.Status == status);
        if (type is not null) query = query.Where(p => p.Type == type);

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{q}%") ||
                EF.Functions.ILike(p.Area ?? "", $"%{q}%"));
        }

        var results = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(p => new PropertyResponse(
             p.Id, p.Name, p.Type, p.Status, p.OwnerId,
             p.AddressLine, p.Area, p.City, p.Region, p.Landmark,
             p.GpsLatitude, p.GpsLongitude, p.CreatedAt,
              _db.Units.Count(u => u.PropertyId == p.Id && u.DeletedAt == null)
))
            .ToListAsync();

        return Ok(results);
    }

    [HttpGet("{propertyId:guid}")]
    [Authorize(Policy = "property.read")]
    public async Task<ActionResult<object>> Get(Guid propertyId)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        var query = _db.Properties
            .AsNoTracking()
            .Include(p => p.Units)
            .Where(p => p.Id == propertyId && p.DeletedAt == null);

        if (User.IsInRole("owner"))
            query = query.Where(p => p.OwnerId == callerUserId);

        var property = await query.FirstOrDefaultAsync();
        if (property is null) return NotFound();

        return Ok(new
        {
            property = ToResponse(property),
            units = property.Units.Select(u => new { u.Id, u.UnitNumber, u.Status, u.Bedrooms, u.Bathrooms })
        });
    }


    public record SetPropertyStatusRequest(PropertyStatus Status);

    [HttpPatch("{propertyId:guid}/status")]
    [Authorize(Policy = "property.update")]
    public async Task<IActionResult> SetStatus(Guid propertyId, [FromBody] SetPropertyStatusRequest req)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        var query = _db.Properties
            .Where(p => p.Id == propertyId && p.DeletedAt == null);

        if (User.IsInRole("owner"))
            query = query.Where(p => p.OwnerId == callerUserId);

        var property = await query.FirstOrDefaultAsync();
        if (property is null) return NotFound();

        property.Status = req.Status;
        property.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        try { await _timeline.WritePropertyEventAsync(property.Id, "PropertyStatusChanged", new { req.Status }); }
        catch { }

        return NoContent();
    }


    [HttpDelete("{propertyId:guid}")]
    [Authorize(Policy = "property.delete")]
    public async Task<IActionResult> Delete(Guid propertyId)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        var query = _db.Properties.Where(p => p.Id == propertyId && p.DeletedAt == null);

        if (User.IsInRole("owner"))
            query = query.Where(p => p.OwnerId == callerUserId);

        var property = await query.FirstOrDefaultAsync();
        if (property is null) return NotFound();

        property.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _timeline.WritePropertyEventAsync(property.Id, "PropertyDeleted");

        return NoContent();
    }

    [HttpGet("{propertyId:guid}/units/{unitId:guid}/belongs")]
    [Authorize] // or [Authorize(Policy="property.internal.read")] if you want stricter
    public async Task<ActionResult<bool>> UnitBelongs(Guid propertyId, Guid unitId, CancellationToken ct)
    {
        var exists = await _db.Units.AsNoTracking()
            .AnyAsync(u => u.Id == unitId && u.PropertyId == propertyId && u.DeletedAt == null, ct);

        return Ok(exists);
    }



    private static PropertyResponse ToResponse(Property p) => new(
        p.Id, p.Name, p.Type, p.Status, p.OwnerId,
        p.AddressLine, p.Area, p.City, p.Region, p.Landmark,
        p.GpsLatitude, p.GpsLongitude, p.CreatedAt,p.Units?.Count??0
    );
}
