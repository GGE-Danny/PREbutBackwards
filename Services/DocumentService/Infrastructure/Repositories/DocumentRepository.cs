using Microsoft.EntityFrameworkCore;
using DocumentService.Application.Interfaces;
using DocumentService.Domain.Entities;
using DocumentService.Domain.Enums;
using DocumentService.Infrastructure.Persistence;

namespace DocumentService.Infrastructure.Repositories;

public sealed class DocumentRepository : IDocumentRepository
{
    private readonly DocumentDbContext _db;
    public DocumentRepository(DocumentDbContext db) => _db = db;

    public async Task AddAsync(Document document, CancellationToken ct)
        => await _db.Documents.AddAsync(document, ct);

    public Task<Document?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Documents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Document?> GetByIdForUpdateAsync(Guid id, CancellationToken ct)
        => _db.Documents.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<Document>> GetForEntityAsync(
        DocumentFor docFor, Guid entityId, int take, CancellationToken ct)
        => await _db.Documents
            .AsNoTracking()
            .Where(x => x.DocumentFor == docFor && x.EntityId == entityId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(ct);

    public async Task<List<Document>> GetForTenantAsync(Guid tenantUserId, int take, CancellationToken ct)
        => await _db.Documents
            .AsNoTracking()
            .Where(x => x.TenantUserId == tenantUserId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(ct);

    public Task<Document?> FindByChecksumAsync(
        DocumentFor docFor, Guid entityId, string checksum, CancellationToken ct)
        => _db.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.DocumentFor == docFor &&
                x.EntityId == entityId &&
                x.ChecksumSha256 == checksum, ct);
}
