using BookingService.Application.Dtos.Responses;
using BookingService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Controllers;

[Route("api/v1/bookings/{bookingId:guid}/logs")]
public sealed class BookingLogsController : ApiControllerBase
{
    private readonly BookingDbContext _db;
    public BookingLogsController(BookingDbContext db) => _db = db;

    [HttpGet]
    [Authorize(Policy = "booking.read")]
    public async Task<ActionResult<List<BookingLogResponse>>> GetLogs([FromRoute] Guid bookingId, CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        var booking = await _db.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == bookingId, ct);

        if (booking is null)
            return NotFound();

        if (!CanAccessTenant(booking.TenantUserId))
            return Forbid();

        var logs = await _db.BookingLogs
            .AsNoTracking()
            .Where(x => x.BookingId == bookingId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new BookingLogResponse(
                x.Id,
                x.BookingId,
                x.EventType,
                x.Notes,
                x.ActorUserId,
                x.CreatedAt
            ))
            .ToListAsync(ct);

        return Ok(logs);
    }
}
