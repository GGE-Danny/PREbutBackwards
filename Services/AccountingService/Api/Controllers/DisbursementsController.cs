using AccountingService.Application.Dtos.Requests;
using AccountingService.Application.Dtos.Responses;
using AccountingService.Application.Interfaces;
using AccountingService.Domain.Entities;
using AccountingService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingService.Api.Controllers;

[Route("api/v1/disbursements")]
public sealed class DisbursementsController : ApiControllerBase
{
    private readonly IOwnerDisbursementRepository _disbursements;
    private readonly IInvoiceRepository _invoices;
    private readonly IExpenseRepository _expenses;
    private readonly ILedgerRepository _ledger;
    private readonly ICommissionCalculator _commissionCalculator;
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _config;

    public DisbursementsController(
        IOwnerDisbursementRepository disbursements,
        IInvoiceRepository invoices,
        IExpenseRepository expenses,
        ILedgerRepository ledger,
        ICommissionCalculator commissionCalculator,
        IUnitOfWork uow,
        IConfiguration config)
    {
        _disbursements = disbursements;
        _invoices = invoices;
        _expenses = expenses;
        _ledger = ledger;
        _commissionCalculator = commissionCalculator;
        _uow = uow;
        _config = config;
    }

    [HttpPost("generate")]
    [Authorize(Policy = "accounting.manage")]
    public async Task<ActionResult<GenerateDisbursementResponse>> Generate(
        [FromBody] GenerateDisbursementRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var actorId))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        // Validate period
        if (req.PeriodEnd < req.PeriodStart)
            return BadRequest("PeriodEnd must be >= PeriodStart.");

        // Resolve commission percent
        var commissionPercent = req.CommissionPercent
            ?? _config.GetValue<decimal>("Commission:DefaultPercent");

        if (commissionPercent < 0 || commissionPercent > 100)
            return BadRequest("CommissionPercent must be between 0 and 100.");

        // Idempotency check
        var existing = await _disbursements.FindByOwnerPropertyPeriodAsync(
            req.OwnerId, req.PropertyId, req.PeriodStart, req.PeriodEnd, ct);

        if (existing is not null)
        {
            // Return existing disbursement (calculate breakdown for transparency)
            var existingBreakdown = await CalculateBreakdownAsync(
                req.PropertyId, req.PeriodStart, req.PeriodEnd, commissionPercent, ct);

            return Ok(new GenerateDisbursementResponse(
                ToResponse(existing),
                existingBreakdown
            ));
        }

        // Calculate amounts
        var rentCollected = await _invoices.SumRentCollectedAsync(
            req.PropertyId, req.PeriodStart, req.PeriodEnd, ct);

        var expensesAmount = await _expenses.SumExpensesAsync(
            req.PropertyId, req.PeriodStart, req.PeriodEnd, ct);

        var commissionAmount = Math.Round(rentCollected * (commissionPercent / 100m), 2);
        var disbursementAmount = _commissionCalculator.CalculateDisbursementAmount(
            rentCollected, expensesAmount, commissionPercent);

        await _uow.BeginTransactionAsync(ct);

        try
        {
            var disbursement = new OwnerDisbursement(
                ownerId: req.OwnerId,
                propertyId: req.PropertyId,
                amount: disbursementAmount,
                periodStart: req.PeriodStart,
                periodEnd: req.PeriodEnd,
                isPaid: false
            );

            await _disbursements.AddAsync(disbursement, ct);

            await _ledger.AddAsync(new LedgerEntry(
                entryType: LedgerEntryType.Disbursement,
                amount: disbursementAmount,
                invoiceId: null,
                paymentId: null,
                expenseId: null,
                ownerDisbursementId: disbursement.Id,
                notes: $"Owner disbursement. Rent:{rentCollected}, Expenses:{expensesAmount}, Commission:{commissionPercent}%={commissionAmount}. Actor:{actorId}"
            ), ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            var breakdown = new DisbursementCalculationBreakdownResponse(
                RentCollected: rentCollected,
                Expenses: expensesAmount,
                CommissionPercent: commissionPercent,
                CommissionAmount: commissionAmount,
                DisbursementAmount: disbursementAmount
            );

            return Ok(new GenerateDisbursementResponse(
                ToResponse(disbursement),
                breakdown
            ));
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "accounting.read")]
    public async Task<ActionResult<OwnerDisbursementResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var disbursement = await _disbursements.GetByIdAsync(id, ct);
        if (disbursement is null)
            return NotFound();

        return Ok(ToResponse(disbursement));
    }

    [HttpPatch("{id:guid}/paid")]
    [Authorize(Policy = "accounting.manage")]
    public async Task<ActionResult<OwnerDisbursementResponse>> MarkPaid(
        [FromRoute] Guid id,
        [FromBody] MarkDisbursementPaidRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var actorId))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var disbursement = await _disbursements.GetByIdForUpdateAsync(id, ct);
        if (disbursement is null)
            return NotFound();

        if (disbursement.IsPaid == req.IsPaid)
            return Ok(ToResponse(disbursement)); // No change needed

        await _uow.BeginTransactionAsync(ct);

        try
        {
            disbursement.IsPaid = req.IsPaid;
            disbursement.UpdatedAt = DateTime.UtcNow;

            // Log the status change in ledger (amount=0, just audit note)
            var statusNote = req.IsPaid ? "marked as PAID" : "marked as UNPAID";
            var notes = $"Disbursement {statusNote}. Actor:{actorId}";
            if (!string.IsNullOrWhiteSpace(req.Notes))
                notes += $" Notes:{req.Notes}";

            await _ledger.AddAsync(new LedgerEntry(
                entryType: LedgerEntryType.Disbursement,
                amount: 0m,
                invoiceId: null,
                paymentId: null,
                expenseId: null,
                ownerDisbursementId: disbursement.Id,
                notes: notes
            ), ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            return Ok(ToResponse(disbursement));
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }

    private async Task<DisbursementCalculationBreakdownResponse> CalculateBreakdownAsync(
        Guid propertyId, DateOnly periodStart, DateOnly periodEnd, decimal commissionPercent, CancellationToken ct)
    {
        var rentCollected = await _invoices.SumRentCollectedAsync(propertyId, periodStart, periodEnd, ct);
        var expensesAmount = await _expenses.SumExpensesAsync(propertyId, periodStart, periodEnd, ct);
        var commissionAmount = Math.Round(rentCollected * (commissionPercent / 100m), 2);
        var disbursementAmount = _commissionCalculator.CalculateDisbursementAmount(
            rentCollected, expensesAmount, commissionPercent);

        return new DisbursementCalculationBreakdownResponse(
            RentCollected: rentCollected,
            Expenses: expensesAmount,
            CommissionPercent: commissionPercent,
            CommissionAmount: commissionAmount,
            DisbursementAmount: disbursementAmount
        );
    }

    private static OwnerDisbursementResponse ToResponse(OwnerDisbursement d) => new(
        d.Id,
        d.OwnerId,
        d.PropertyId,
        d.Amount,
        d.PeriodStart,
        d.PeriodEnd,
        d.IsPaid,
        d.CreatedAt
    );
}
