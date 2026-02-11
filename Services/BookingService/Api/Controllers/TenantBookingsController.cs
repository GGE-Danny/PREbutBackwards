using BookingService.Application.Dtos.Responses;
using BookingService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Controllers;

[Route("api/v1/tenants/{tenantUserId:guid}/bookings")]
public sealed class TenantBookingsController : ApiControllerBase
{
    private readonly BookingDbContext _db;

    public TenantBookingsController(BookingDbContext db) => _db = db;

    [HttpGet]
    [Authorize(Policy = "booking.read")]
    public async Task<ActionResult<List<BookingResponse>>> GetTenantBookings([FromRoute] Guid tenantUserId, CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (!CanAccessTenant(tenantUserId))
            return Forbid();

        var bookings = await _db.Bookings
            .AsNoTracking()
            .Where(x => x.TenantUserId == tenantUserId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new BookingResponse(
                x.Id,
                x.TenantUserId,
                x.PropertyId,
                x.UnitId,
                x.StartDate,
                x.EndDate,
                x.Status,
                x.Notes,
                x.CreatedAt,
                x.UpdatedAt
            ))
            .ToListAsync(ct);

        return Ok(bookings);
    }
}
