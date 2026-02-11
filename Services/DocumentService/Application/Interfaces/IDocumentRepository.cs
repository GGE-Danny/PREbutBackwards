using DocumentService.Domain.Entities;
using DocumentService.Domain.Enums;

namespace DocumentService.Application.Interfaces;

public interface IDocumentRepository
{
    Task AddAsync(Document document, CancellationToken ct);
    Task<Document?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Document?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);
    Task<List<Document>> GetForEntityAsync(DocumentFor docFor, Guid entityId, int take, CancellationToken ct);
    Task<List<Document>> GetForTenantAsync(Guid tenantUserId, int take, CancellationToken ct);
    Task<Document?> FindByChecksumAsync(DocumentFor docFor, Guid entityId, string checksum, CancellationToken ct);
}
