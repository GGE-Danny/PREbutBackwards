using TenantService.Domain.Common;

namespace TenantService.Domain.Entities;

public class TenantDocument : BaseEntity
{
    public Guid TenantProfileId { get; set; }
    public TenantProfile TenantProfile { get; set; } = default!;

    public string DocumentId { get; set; } = default!;  // DocumentService id
    public string Type { get; set; } = "Other";         // IDCard/Passport/Contract/Etc
    public string? Title { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? TagsCsv { get; set; }

    public Guid? UploadedByUserId { get; set; }
}
