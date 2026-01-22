using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TenantService.Application.DTOs;
using TenantService.Domain.Entities;
using TenantService.Infrastructure.Persistence;

namespace TenantService.Api.Controllers;

[ApiController]
[Route("api/v1/tenants")]
public class TenantDocumentsController : ControllerBase
{
    private readonly TenantDbContext _db;
    public TenantDocumentsController(TenantDbContext db) => _db = db;

    private bool TryGetCallerUserId(out Guid userId)
    {
        userId = default;

        var id =
            User.FindFirstValue("sub") ??
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(id, out userId);
    }

    private static bool IsTenant(ClaimsPrincipal user) => user.IsInRole("tenant");
    private static bool CanManage(ClaimsPrincipal user)
        => user.IsInRole("super_admin") || user.IsInRole("manager") || user.IsInRole("support") || user.IsInRole("sales");

    // POST /api/v1/tenants/{tenantUserId}/documents
    [HttpPost("{tenantUserId:guid}/documents")]
    [Authorize]
    public async Task<ActionResult<TenantDocumentResponse>> Add(Guid tenantUserId, [FromBody] AddTenantDocumentRequest req)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        var isTenant = IsTenant(User);
        var canManage = CanManage(User);

        // tenant can only add to self
        if (isTenant && tenantUserId != callerUserId)
            return Forbid("Tenants can only add documents to their own profile.");

        if (!isTenant && !canManage)
            return Forbid("Not allowed.");

        var profile = await _db.TenantProfiles
            .FirstOrDefaultAsync(x => x.TenantUserId == tenantUserId);

        if (profile is null)
            return NotFound("Tenant profile not found. Create profile first.");

        Guid? uploaderId = callerUserId;

        var doc = new TenantDocument
        {
            TenantProfileId = profile.Id,
            DocumentId = req.DocumentId,
            Type = string.IsNullOrWhiteSpace(req.Type) ? "Other" : req.Type,
            Title = req.Title,
            ExpiryDate = req.ExpiryDate,
            TagsCsv = req.TagsCsv,
            UploadedByUserId = uploaderId
        };

        _db.TenantDocuments.Add(doc);
        await _db.SaveChangesAsync();

        return Ok(ToResponse(doc));
    }

    // GET /api/v1/tenants/{tenantUserId}/documents
    [HttpGet("{tenantUserId:guid}/documents")]
    [Authorize]
    public async Task<ActionResult<List<TenantDocumentResponse>>> List(Guid tenantUserId)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        var isTenant = IsTenant(User);
        var canManage = CanManage(User);

        if (isTenant && tenantUserId != callerUserId)
            return Forbid();

        if (!isTenant && !canManage)
            return Forbid();

        var profile = await _db.TenantProfiles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantUserId == tenantUserId);

        if (profile is null) return Ok(new List<TenantDocumentResponse>());

        var docs = await _db.TenantDocuments.AsNoTracking()
            .Where(x => x.TenantProfileId == profile.Id && x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => ToResponse(x))
            .ToListAsync();

        return Ok(docs);
    }

    // DELETE (soft) /api/v1/tenants/documents/{documentRowId}
    [HttpDelete("documents/{documentRowId:guid}")]
    [Authorize]
    public async Task<IActionResult> SoftDelete(Guid documentRowId)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        var isTenant = IsTenant(User);
        var canManage = CanManage(User);

        var doc = await _db.TenantDocuments
            .Include(d => d.TenantProfile)
            .FirstOrDefaultAsync(x => x.Id == documentRowId && x.DeletedAt == null);

        if (doc is null) return NotFound();

        // tenant can only delete their own docs; staff can delete any
        if (isTenant && doc.TenantProfile.TenantUserId != callerUserId)
            return Forbid();

        if (!isTenant && !canManage)
            return Forbid();

        doc.DeletedAt = DateTime.UtcNow;
        doc.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static TenantDocumentResponse ToResponse(TenantDocument d) => new(
        d.Id, d.TenantProfileId, d.DocumentId, d.Type, d.Title,
        d.ExpiryDate, d.TagsCsv, d.UploadedByUserId, d.CreatedAt
    );
}
