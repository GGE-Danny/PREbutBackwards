using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TenantService.Application.DTOs;
using TenantService.Infrastructure.Persistence;

namespace TenantService.Api.Controllers;

[ApiController]
[Route("api/v1/tenants")]
public class TenantResidenciesController : ControllerBase
{
    private readonly TenantDbContext _db;

    public TenantResidenciesController(TenantDbContext db) => _db = db;

    // GET /api/v1/tenants/{tenantUserId}/residencies
    [HttpGet("{tenantUserId:guid}/residencies")]
    [Authorize(Policy = "tenant.read")]
    public async Task<ActionResult<List<TenantResidencyResponse>>> List(Guid tenantUserId)
    {
        var items = await _db.TenantResidencies.AsNoTracking()
            .Where(x => x.TenantUserId == tenantUserId)
            .OrderByDescending(x => x.MoveInDate)
            .Select(x => new TenantResidencyResponse(
                x.Id, x.TenantUserId, x.PropertyId, x.UnitId,
                x.MoveInDate, x.MoveOutDate, x.Source, x.Notes, x.CreatedAt))
            .ToListAsync();

        return Ok(items);
    }
}
