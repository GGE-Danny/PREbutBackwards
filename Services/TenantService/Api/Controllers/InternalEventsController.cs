using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TenantService.Application.DTOs;
using TenantService.Domain.Entities;
using TenantService.Infrastructure.Persistence;

namespace TenantService.Api.Controllers;

[ApiController]
[Route("api/v1/internal")]
public class InternalEventsController : ControllerBase
{
    private readonly TenantDbContext _db;

    public InternalEventsController(TenantDbContext db) => _db = db;

    // PropertyService calls this when occupancy is assigned
    [HttpPost("occupancy-assigned")]
    [Authorize(Policy = "tenant.internal.write")]
    public async Task<IActionResult> OccupancyAssigned([FromBody] OccupancyAssignedEvent evt)
    {
        // Idempotent protection: if already exists (same tenant + unit + moveInDate), do nothing
        var exists = await _db.TenantResidencies.AnyAsync(x =>
            x.TenantUserId == evt.TenantUserId &&
            x.UnitId == evt.UnitId &&
            x.MoveInDate == evt.MoveInDate);

        if (!exists)
        {
            _db.TenantResidencies.Add(new TenantResidencyHistory
            {
                TenantUserId = evt.TenantUserId,
                PropertyId = evt.PropertyId,
                UnitId = evt.UnitId,
                MoveInDate = evt.MoveInDate,
                MoveOutDate = null,
                Source = "occupancy",
                Notes = evt.Notes
            });

            await _db.SaveChangesAsync();
        }

        return Ok(new { status = "recorded" });
    }

    // PropertyService calls this when occupancy is vacated
    [HttpPost("occupancy-vacated")]
    [Authorize(Policy = "tenant.internal.write")]
    public async Task<IActionResult> OccupancyVacated([FromBody] OccupancyVacatedEvent evt)
    {
        // Find latest active residency entry for this tenant+unit
        var row = await _db.TenantResidencies
            .Where(x =>
                x.TenantUserId == evt.TenantUserId &&
                x.UnitId == evt.UnitId &&
                x.MoveOutDate == null)
            .OrderByDescending(x => x.MoveInDate)
            .FirstOrDefaultAsync();

        if (row is null)
        {
            // no active record: create a closed record for completeness (optional behavior)
            _db.TenantResidencies.Add(new TenantResidencyHistory
            {
                TenantUserId = evt.TenantUserId,
                PropertyId = evt.PropertyId,
                UnitId = evt.UnitId,
                MoveInDate = evt.MoveOutDate, // fallback
                MoveOutDate = evt.MoveOutDate,
                Source = "occupancy",
                Notes = evt.Notes
            });

            await _db.SaveChangesAsync();
            return Ok(new { status = "created_closed_record" });
        }

        row.MoveOutDate = evt.MoveOutDate;
        row.Notes = evt.Notes ?? row.Notes;
        row.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new { status = "closed" });
    }
}
