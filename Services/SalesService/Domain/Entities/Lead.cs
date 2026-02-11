using SalesService.Domain.Common;
using SalesService.Domain.Enums;

namespace SalesService.Domain.Entities;

public class Lead : BaseEntity
{
    public Guid? TenantUserId { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Email { get; set; }
    public string Source { get; set; } = null!;
    public Guid PropertyId { get; set; }
    public Guid? UnitId { get; set; }
    public string? Notes { get; set; }
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public Guid? AssignedToUserId { get; set; }
    public Guid CreatedByUserId { get; set; }

    // Navigation
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
