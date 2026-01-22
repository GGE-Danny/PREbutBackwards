using TenantService.Domain.Common;

namespace TenantService.Domain.Entities;

public class TenantProfile : BaseEntity
{
    public Guid TenantUserId { get; set; } // Auth userId (sub) - UNIQUE

    public string FullName { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }

    // ID / KYC
    public string? NationalIdType { get; set; }     // e.g. "Passport", "NIN"
    public string? NationalIdNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Nationality { get; set; }

    // Employment
    public string? EmploymentStatus { get; set; }   // Employed/Self-employed/Unemployed
    public string? EmployerName { get; set; }
    public string? JobTitle { get; set; }

    // Extra
    public string? CurrentAddress { get; set; }
    public string? NextOfKinName { get; set; }
    public string? NextOfKinPhone { get; set; }

    public string? Notes { get; set; }

    public List<TenantDocument> Documents { get; set; } = new();
}
