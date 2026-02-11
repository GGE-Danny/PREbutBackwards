using DocumentService.Domain.Enums;

namespace DocumentService.Application.Dtos.Responses;

public record DocumentResponse(
    Guid Id,
    DocumentFor DocumentFor,
    Guid EntityId,
    Guid? TenantUserId,
    DocumentType Type,
    DocumentVisibility Visibility,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? ChecksumSha256,
    DateTime? ExpiresAt,
    DateTime CreatedAt,
    Guid UploadedByUserId
);
