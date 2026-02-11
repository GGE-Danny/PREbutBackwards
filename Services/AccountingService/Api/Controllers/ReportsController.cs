using AccountingService.Application.Dtos.Responses;
using AccountingService.Domain.Enums;
using AccountingService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Api.Controllers;

[Route("api/v1/reports")]
public sealed class ReportsController : ApiControllerBase
{
    private readonly AccountingDbContext _db;
    public ReportsController(AccountingDbContext db) => _db = db;

    [HttpGet("revenue")]
    [Authorize(Policy = "accounting.read")]
    public async Task<ActionResult<FinancialReportResponse>> Revenue(
        [FromQuery] string period, // "monthly" or "quarterly"
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (toDate < fromDate)
            return BadRequest("toDate must be >= fromDate.");

        var fromDt = fromDate.ToDateTime(TimeOnly.MinValue);
        var toDt = toDate.ToDateTime(TimeOnly.MaxValue);

        var revenue = await _db.Payments.AsNoTracking()
            .Where(x => x.CreatedAt >= fromDt && x.CreatedAt <= toDt)
            .SumAsync(x => x.Amount, ct);

        var expenses = await _db.Expenses.AsNoTracking()
            .Where(x => x.IncurredAt >= fromDt && x.IncurredAt <= toDt)
            .SumAsync(x => x.Amount, ct);

        return Ok(new FinancialReportResponse(
            Period: period,
            FromDate: fromDate,
            ToDate: toDate,
            TotalRevenue: revenue,
            TotalExpenses: expenses,
            NetRevenue: revenue - expenses
        ));
    }
}
