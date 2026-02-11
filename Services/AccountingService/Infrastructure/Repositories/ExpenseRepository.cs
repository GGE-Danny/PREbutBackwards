using AccountingService.Application.Interfaces;
using AccountingService.Domain.Entities;
using AccountingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Infrastructure.Repositories;

public sealed class ExpenseRepository : IExpenseRepository
{
    private readonly AccountingDbContext _db;
    public ExpenseRepository(AccountingDbContext db) => _db = db;

    public async Task AddAsync(Expense expense, CancellationToken ct)
        => await _db.Expenses.AddAsync(expense, ct);

    public Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Expenses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Expense>> GetByPropertyAsync(Guid propertyId, DateTime from, DateTime to, CancellationToken ct)
        => await _db.Expenses.AsNoTracking()
            .Where(x => x.PropertyId == propertyId && x.IncurredAt >= from && x.IncurredAt <= to)
            .OrderByDescending(x => x.IncurredAt)
            .ToListAsync(ct);

    public Task<decimal> SumExpensesAsync(Guid propertyId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        // Must use UTC for PostgreSQL timestamptz columns
        var fromDt = DateTime.SpecifyKind(from.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var toDt = DateTime.SpecifyKind(to.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        return _db.Expenses.AsNoTracking()
            .Where(x => x.PropertyId == propertyId && x.IncurredAt >= fromDt && x.IncurredAt <= toDt)
            .SumAsync(x => x.Amount, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
