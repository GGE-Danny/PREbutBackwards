using AccountingService.Application.Interfaces;
using AccountingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace AccountingService.Infrastructure;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AccountingDbContext _db;
    private IDbContextTransaction? _tx;

    public UnitOfWork(AccountingDbContext db) => _db = db;

    public async Task BeginTransactionAsync(CancellationToken ct)
        => _tx = await _db.Database.BeginTransactionAsync(ct);

    public Task<int> SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);

    public async Task CommitAsync(CancellationToken ct)
    {
        if (_tx is null) return;
        await _tx.CommitAsync(ct);
        await _tx.DisposeAsync();
        _tx = null;
    }

    public async Task RollbackAsync(CancellationToken ct)
    {
        if (_tx is null) return;
        await _tx.RollbackAsync(ct);
        await _tx.DisposeAsync();
        _tx = null;
    }
}
