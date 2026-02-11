using DocumentService.Domain.Enums;

namespace DocumentService.Application.Dtos.Requests;

public record UpdateDocumentRequest(
    DocumentType? Type,
    DocumentVisibility? Visibility,
    string? Notes,
    DateTime? ExpiresAt
);
