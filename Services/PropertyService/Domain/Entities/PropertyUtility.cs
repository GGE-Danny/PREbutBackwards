using PropertyService.Domain.Common;

namespace PropertyService.Domain.Entities;

public class PropertyUtility : BaseEntity
{
 //   public Guid TenantId { get; set; }
    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = default!;

    public string UtilityType { get; set; } = "Electricity"; // keep flexible
    public string? PoleNumber { get; set; }
    public string? MeterNumber { get; set; }
    public string? Provider { get; set; }
    public bool IsActive { get; set; } = true;
}
