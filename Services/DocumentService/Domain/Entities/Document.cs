using DocumentService.Domain.Common;
using DocumentService.Domain.Enums;

namespace DocumentService.Domain.Entities;

public class Document : BaseEntity
{
    public DocumentFor DocumentFor { get; set; }
    public Guid EntityId { get; set; }
    public Guid? TenantUserId { get; set; }
    public Guid UploadedByUserId { get; set; }
    public DocumentType Type { get; set; }
    public DocumentVisibility Visibility { get; set; } = DocumentVisibility.Private;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public string StoragePath { get; set; } = null!;
    public string? ChecksumSha256 { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Notes { get; set; }
}
