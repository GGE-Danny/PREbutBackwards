using AccountingService.Domain.Entities;

namespace AccountingService.Application.Interfaces;

public interface IExpenseRepository
{
    Task AddAsync(Expense expense, CancellationToken ct);
    Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Expense>> GetByPropertyAsync(Guid propertyId, DateTime from, DateTime to, CancellationToken ct);
    Task<decimal> SumExpensesAsync(Guid propertyId, DateOnly from, DateOnly to, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
