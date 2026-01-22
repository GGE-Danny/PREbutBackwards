using TenantService.Domain.Common;

namespace TenantService.Domain.Entities;

public class TenantMeterAssociation : BaseEntity
{
    public Guid TenantUserId { get; set; }

    public Guid PropertyId { get; set; }
    public Guid UnitId { get; set; }

    public string MeterNumber { get; set; } = default!;
    public string? PoleNumber { get; set; }
    public string? Provider { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; } // null = active
}
