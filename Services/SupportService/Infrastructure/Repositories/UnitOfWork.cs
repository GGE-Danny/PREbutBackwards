using Microsoft.EntityFrameworkCore.Storage;
using SupportService.Application.Interfaces;
using SupportService.Infrastructure.Persistence;

namespace SupportService.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly SupportDbContext _db;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(SupportDbContext db) => _db = db;

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _db.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
