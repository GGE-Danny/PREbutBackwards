using AccountingService.Application.Dtos.Requests;
using AccountingService.Application.Dtos.Responses;
using AccountingService.Application.Interfaces;
using AccountingService.Domain.Entities;
using AccountingService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingService.Api.Controllers;

[Route("api/v1/expenses")]
public sealed class ExpensesController : ApiControllerBase
{
    private readonly IExpenseRepository _expenses;
    private readonly ILedgerRepository _ledger;
    private readonly IUnitOfWork _uow;

    public ExpensesController(
        IExpenseRepository expenses,
        ILedgerRepository ledger,
        IUnitOfWork uow)
    {
        _expenses = expenses;
        _ledger = ledger;
        _uow = uow;
    }

    [HttpPost]
    [Authorize(Policy = "accounting.write")]
    public async Task<ActionResult<ExpenseResponse>> LogExpense([FromBody] LogExpenseRequest req, CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var actorId))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        // Validate Amount
        if (req.Amount <= 0)
            return BadRequest("Amount must be > 0.");

        // Validate Description
        if (string.IsNullOrWhiteSpace(req.Description))
            return BadRequest("Description is required.");

        // Validate IncurredAt not in far future (allow +1 day tolerance)
        if (req.IncurredAt > DateTime.UtcNow.AddDays(1))
            return BadRequest("IncurredAt cannot be more than 1 day in the future.");

        await _uow.BeginTransactionAsync(ct);

        try
        {
            var expense = new Expense(
                propertyId: req.PropertyId,
                amount: req.Amount,
                category: req.Category,
                description: req.Description,
                incurredAt: req.IncurredAt
            );

            await _expenses.AddAsync(expense, ct);

            await _ledger.AddAsync(new LedgerEntry(
                entryType: LedgerEntryType.Expense,
                amount: expense.Amount,
                invoiceId: null,
                paymentId: null,
                expenseId: expense.Id,
                ownerDisbursementId: null,
                notes: $"Expense: {expense.Category} - {expense.Description}. Actor:{actorId}"
            ), ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            return Ok(ToResponse(expense));
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }

    [HttpGet("{expenseId:guid}")]
    [Authorize(Policy = "accounting.read")]
    public async Task<ActionResult<ExpenseResponse>> GetById([FromRoute] Guid expenseId, CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var expense = await _expenses.GetByIdAsync(expenseId, ct);
        if (expense is null)
            return NotFound();

        return Ok(ToResponse(expense));
    }

    [HttpGet]
    [Authorize(Policy = "accounting.read")]
    public async Task<ActionResult<IReadOnlyList<ExpenseResponse>>> GetByProperty(
        [FromQuery] Guid propertyId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        if (from > to)
            return BadRequest("from must be <= to.");

        var expenses = await _expenses.GetByPropertyAsync(propertyId, from, to, ct);

        return Ok(expenses.Select(ToResponse).ToList());
    }

    private static ExpenseResponse ToResponse(Expense e) => new(
        e.Id,
        e.PropertyId,
        e.Amount,
        e.Category,
        e.Description,
        e.IncurredAt,
        e.CreatedAt
    );
}
