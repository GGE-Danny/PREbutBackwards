using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TenantService.Application.DTOs;
using TenantService.Domain.Entities;
using TenantService.Infrastructure.Persistence;

namespace TenantService.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/profile")]
public class TenantProfilesController : ControllerBase
{
    private readonly TenantDbContext _db;

    public TenantProfilesController(TenantDbContext db) => _db = db;

    private bool TryGetCallerUserId(out Guid userId)
    {
        userId = default;

        var id =
            User.FindFirstValue("sub") ??
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(id, out userId);
    }


    // Create profile
    // Tenants can only create for themselves
    // Staff can create for any TenantUserId (helping tenants)
    [HttpPost]
    [Authorize] // role logic inside
    public async Task<ActionResult<TenantProfileResponse>> Create([FromBody] CreateTenantProfileRequest req)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        var isTenant = User.IsInRole("tenant");
        var canManage = User.IsInRole("super_admin") || User.IsInRole("manager") || User.IsInRole("support") || User.IsInRole("sales");

        if (isTenant && req.TenantUserId != callerUserId)
            return Forbid("Tenants can only create their own profile.");

        if (!isTenant && !canManage)
            return Forbid("Not allowed.");

        // ✅ tenants always create for themselves
        var targetTenantUserId = isTenant ? callerUserId : req.TenantUserId;

        var exists = await _db.TenantProfiles.AnyAsync(x => x.TenantUserId == targetTenantUserId);
        if (exists) return Conflict("Profile already exists for this user.");

        var entity = new TenantProfile
        {
            TenantUserId = targetTenantUserId,
            FullName = req.FullName,
            PhoneNumber = req.PhoneNumber,
            Email = req.Email,
            NationalIdType = req.NationalIdType,
            NationalIdNumber = req.NationalIdNumber,
            DateOfBirth = req.DateOfBirth,
            Nationality = req.Nationality,
            EmploymentStatus = req.EmploymentStatus,
            EmployerName = req.EmployerName,
            JobTitle = req.JobTitle,
            CurrentAddress = req.CurrentAddress,
            NextOfKinName = req.NextOfKinName,
            NextOfKinPhone = req.NextOfKinPhone,
            Notes = req.Notes
        };

        _db.TenantProfiles.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(ToResponse(entity));
    }

    // Get profile by tenantUserId
    // Tenant can only view their own
    // Staff can view anyone
    [HttpGet("{tenantUserId:guid}")]
    [Authorize]
    public async Task<ActionResult<TenantProfileResponse>> Get(Guid tenantUserId)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        var isTenant = User.IsInRole("tenant");
        var canManage = User.IsInRole("super_admin") || User.IsInRole("manager") || User.IsInRole("support") || User.IsInRole("sales");

        if (isTenant && tenantUserId != callerUserId)
            return Forbid();

        if (!isTenant && !canManage)
            return Forbid();

        var profile = await _db.TenantProfiles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantUserId == tenantUserId);

        if (profile is null) return NotFound();

        return Ok(ToResponse(profile));
    }

    // Update profile
    // Tenant can update their own
    // Staff can update anyone
    [HttpPatch("{tenantUserId:guid}")]
    [Authorize]
    public async Task<ActionResult<TenantProfileResponse>> Update(Guid tenantUserId, [FromBody] UpdateTenantProfileRequest req)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized("Invalid user id in token.");

        var isTenant = User.IsInRole("tenant");
        var canManage = User.IsInRole("super_admin") || User.IsInRole("manager") || User.IsInRole("support") || User.IsInRole("sales");

        if (isTenant && tenantUserId != callerUserId)
            return Forbid();

        if (!isTenant && !canManage)
            return Forbid();

        var profile = await _db.TenantProfiles
            .FirstOrDefaultAsync(x => x.TenantUserId == tenantUserId);

        if (profile is null) return NotFound();

        profile.FullName = req.FullName ?? profile.FullName;
        profile.PhoneNumber = req.PhoneNumber ?? profile.PhoneNumber;
        profile.Email = req.Email ?? profile.Email;

        profile.NationalIdType = req.NationalIdType ?? profile.NationalIdType;
        profile.NationalIdNumber = req.NationalIdNumber ?? profile.NationalIdNumber;
        profile.DateOfBirth = req.DateOfBirth ?? profile.DateOfBirth;
        profile.Nationality = req.Nationality ?? profile.Nationality;

        profile.EmploymentStatus = req.EmploymentStatus ?? profile.EmploymentStatus;
        profile.EmployerName = req.EmployerName ?? profile.EmployerName;
        profile.JobTitle = req.JobTitle ?? profile.JobTitle;

        profile.CurrentAddress = req.CurrentAddress ?? profile.CurrentAddress;
        profile.NextOfKinName = req.NextOfKinName ?? profile.NextOfKinName;
        profile.NextOfKinPhone = req.NextOfKinPhone ?? profile.NextOfKinPhone;
        profile.Notes = req.Notes ?? profile.Notes;

        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(profile));
    }

    // Search (staff only)
    [HttpGet("/api/v1/tenants/search")]
    [Authorize(Policy = "tenant.manage")]
    public async Task<ActionResult<List<TenantProfileResponse>>> Search([FromQuery] string q)
    {
        q = q?.Trim() ?? "";
        if (q.Length < 2) return BadRequest("Query too short.");

        var results = await _db.TenantProfiles.AsNoTracking()
            .Where(x =>
                EF.Functions.ILike(x.FullName, $"%{q}%") ||
                EF.Functions.ILike(x.PhoneNumber ?? "", $"%{q}%") ||
                EF.Functions.ILike(x.Email ?? "", $"%{q}%") ||
                EF.Functions.ILike(x.NationalIdNumber ?? "", $"%{q}%"))
            .OrderBy(x => x.FullName)
            .Take(50)
            .ToListAsync();

        return Ok(results.Select(ToResponse).ToList());
    }

    private static TenantProfileResponse ToResponse(TenantProfile p) => new(
        p.Id,
        p.TenantUserId,
        p.FullName,
        p.PhoneNumber,
        p.Email,
        p.NationalIdType,
        p.NationalIdNumber,
        p.DateOfBirth,
        p.Nationality,
        p.EmploymentStatus,
        p.EmployerName,
        p.JobTitle,
        p.CurrentAddress,
        p.NextOfKinName,
        p.NextOfKinPhone,
        p.Notes,
        p.CreatedAt
    );
}
