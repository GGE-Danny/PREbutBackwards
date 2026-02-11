using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DocumentService.Application.Dtos.Requests;
using DocumentService.Application.Dtos.Responses;
using DocumentService.Application.Interfaces;
using DocumentService.Domain.Entities;
using DocumentService.Domain.Enums;

namespace DocumentService.Api.Controllers;

[Route("api/v1/documents")]
public sealed class DocumentsController : ApiControllerBase
{
    private readonly IDocumentRepository _documents;
    private readonly IFileStorage _fileStorage;
    private readonly IUnitOfWork _uow;

    public DocumentsController(
        IDocumentRepository documents,
        IFileStorage fileStorage,
        IUnitOfWork uow)
    {
        _documents = documents;
        _fileStorage = fileStorage;
        _uow = uow;
    }

    /// <summary>
    /// Upload a document.
    /// Tenant: TenantUserId derived from token; only Tenant/Booking allowed.
    /// Staff: Can upload for any entity.
    /// </summary>
    [HttpPost("upload")]
    [Authorize(Policy = "document.write")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DocumentResponse>> Upload(
        [FromForm] UploadDocumentRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var callerId))
            return Unauthorized();

        if (req.File == null || req.File.Length == 0)
            return BadRequest("File is required.");

        // Determine TenantUserId based on role
        Guid? effectiveTenantUserId;

        if (IsTenant(User))
        {
            // Tenant: TenantUserId must be self
            effectiveTenantUserId = callerId;

            // Tenant can only upload for Tenant or Booking
            if (req.DocumentFor != DocumentFor.Tenant && req.DocumentFor != DocumentFor.Booking)
                return BadRequest("Tenants can only upload documents for Tenant or Booking.");
        }
        else if (CanManage(User))
        {
            // Staff: Use provided TenantUserId or null
            effectiveTenantUserId = req.TenantUserId;
        }
        else
        {
            return Forbid();
        }

        // Ensure ExpiresAt is UTC
        DateTime? expiresAtUtc = null;
        if (req.ExpiresAt.HasValue)
        {
            expiresAtUtc = req.ExpiresAt.Value.Kind == DateTimeKind.Utc
                ? req.ExpiresAt.Value
                : DateTime.SpecifyKind(req.ExpiresAt.Value, DateTimeKind.Utc);
        }

        // Save file to disk
        string storagePath, checksumSha256;
        long sizeBytes;

        await using (var stream = req.File.OpenReadStream())
        {
            (storagePath, checksumSha256, sizeBytes) = await _fileStorage.SaveAsync(
                stream, req.File.FileName, req.File.ContentType, ct);
        }

        // Idempotency: check if same file already exists for this entity
        var existing = await _documents.FindByChecksumAsync(req.DocumentFor, req.EntityId, checksumSha256, ct);
        if (existing is not null)
        {
            // Delete the newly uploaded file (duplicate)
            await _fileStorage.DeleteAsync(storagePath, ct);
            return Ok(ToResponse(existing));
        }

        // Create document record
        var document = new Document
        {
            DocumentFor = req.DocumentFor,
            EntityId = req.EntityId,
            TenantUserId = effectiveTenantUserId,
            UploadedByUserId = callerId,
            Type = req.Type,
            Visibility = req.Visibility,
            FileName = req.File.FileName,
            ContentType = req.File.ContentType,
            SizeBytes = sizeBytes,
            StoragePath = storagePath,
            ChecksumSha256 = checksumSha256,
            ExpiresAt = expiresAtUtc,
            Notes = req.Notes
        };

        await _documents.AddAsync(document, ct);
        await _uow.SaveChangesAsync(ct);

        return Ok(ToResponse(document));
    }

    /// <summary>
    /// Get document metadata by ID.
    /// Tenant can only access if TenantUserId matches.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "document.read")]
    public async Task<ActionResult<DocumentResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        var document = await _documents.GetByIdAsync(id, ct);
        if (document is null)
            return NotFound();

        if (!CanAccessDocument(document))
            return Forbid();

        return Ok(ToResponse(document));
    }

    /// <summary>
    /// Download document file.
    /// </summary>
    [HttpGet("{id:guid}/download")]
    [Authorize(Policy = "document.read")]
    public async Task<IActionResult> Download(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        var document = await _documents.GetByIdAsync(id, ct);
        if (document is null)
            return NotFound();

        if (!CanAccessDocument(document))
            return Forbid();

        try
        {
            var stream = await _fileStorage.OpenReadAsync(document.StoragePath, ct);
            return File(stream, document.ContentType, document.FileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound("File not found on disk.");
        }
    }

    /// <summary>
    /// Get documents for an entity (e.g., /api/v1/documents/property/{propertyId}).
    /// </summary>
    [HttpGet("{docFor}/{entityId:guid}")]
    [Authorize(Policy = "document.read")]
    public async Task<ActionResult<List<DocumentResponse>>> GetForEntity(
        [FromRoute] DocumentFor docFor,
        [FromRoute] Guid entityId,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        if (!TryGetCallerUserId(out var callerId))
            return Unauthorized();

        take = Math.Clamp(take, 1, 200);

        var documents = await _documents.GetForEntityAsync(docFor, entityId, take, ct);

        // Filter based on access
        if (IsTenant(User))
        {
            // Tenant can only see documents where TenantUserId matches
            documents = documents
                .Where(d => d.TenantUserId.HasValue && d.TenantUserId.Value == callerId)
                .ToList();
        }

        return Ok(documents.Select(ToResponse).ToList());
    }

    /// <summary>
    /// Update document metadata.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "document.write")]
    public async Task<ActionResult<DocumentResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateDocumentRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var callerId))
            return Unauthorized();

        var document = await _documents.GetByIdForUpdateAsync(id, ct);
        if (document is null)
            return NotFound();

        // Access check
        if (IsTenant(User))
        {
            // Tenant can only update own documents
            if (!document.TenantUserId.HasValue || document.TenantUserId.Value != callerId)
                return Forbid();

            // Tenant can only update Notes and Visibility (to Private)
            if (req.Type.HasValue || (req.Visibility.HasValue && req.Visibility.Value != DocumentVisibility.Private))
                return BadRequest("Tenants can only update Notes and set Visibility to Private.");
        }
        else if (!CanManage(User))
        {
            return Forbid();
        }

        // Apply updates
        if (req.Type.HasValue)
            document.Type = req.Type.Value;

        if (req.Visibility.HasValue)
            document.Visibility = req.Visibility.Value;

        if (req.Notes is not null)
            document.Notes = req.Notes;

        if (req.ExpiresAt.HasValue)
        {
            document.ExpiresAt = req.ExpiresAt.Value.Kind == DateTimeKind.Utc
                ? req.ExpiresAt.Value
                : DateTime.SpecifyKind(req.ExpiresAt.Value, DateTimeKind.Utc);
        }

        document.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        return Ok(ToResponse(document));
    }

    /// <summary>
    /// Soft delete a document.
    /// Tenant can delete own; staff can delete any.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "document.write")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var callerId))
            return Unauthorized();

        var document = await _documents.GetByIdForUpdateAsync(id, ct);
        if (document is null)
            return NotFound();

        // Access check
        if (IsTenant(User))
        {
            if (!document.TenantUserId.HasValue || document.TenantUserId.Value != callerId)
                return Forbid();
        }
        else if (!CanManage(User))
        {
            return Forbid();
        }

        document.DeletedAt = DateTime.UtcNow;
        document.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        // Note: File is NOT deleted from disk (kept for audit)

        return NoContent();
    }

    private bool CanAccessDocument(Document document)
    {
        if (CanManage(User))
            return true;

        if (!TryGetCallerUserId(out var callerId))
            return false;

        // Tenant can only access documents where TenantUserId matches
        if (document.TenantUserId.HasValue && document.TenantUserId.Value == callerId)
            return true;

        // Check visibility
        if (document.Visibility == DocumentVisibility.StaffOnly)
            return false;

        return false;
    }

    private static DocumentResponse ToResponse(Document d) => new(
        d.Id,
        d.DocumentFor,
        d.EntityId,
        d.TenantUserId,
        d.Type,
        d.Visibility,
        d.FileName,
        d.ContentType,
        d.SizeBytes,
        d.ChecksumSha256,
        d.ExpiresAt,
        d.CreatedAt,
        d.UploadedByUserId
    );
}
