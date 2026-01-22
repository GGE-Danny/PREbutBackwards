namespace TenantService.Application.DTOs;

public record AddTenantDocumentRequest(
    string DocumentId,
    string Type,
    string? Title,
    DateOnly? ExpiryDate,
    string? TagsCsv
);

public record TenantDocumentResponse(
    Guid Id,
    Guid TenantProfileId,
    string DocumentId,
    string Type,
    string? Title,
    DateOnly? ExpiryDate,
    string? TagsCsv,
    Guid? UploadedByUserId,
    DateTime CreatedAt
);
