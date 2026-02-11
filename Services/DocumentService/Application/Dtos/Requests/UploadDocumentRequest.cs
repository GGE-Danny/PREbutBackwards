using DocumentService.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace DocumentService.Application.Dtos.Requests;

public class UploadDocumentRequest
{
    public IFormFile File { get; set; } = null!;
    public DocumentFor DocumentFor { get; set; }
    public Guid EntityId { get; set; }
    public Guid? TenantUserId { get; set; }
    public DocumentType Type { get; set; }
    public DocumentVisibility Visibility { get; set; }
    public string? Notes { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
