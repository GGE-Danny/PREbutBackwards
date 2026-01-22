using PropertyService.Domain.Common;
using PropertyService.Domain.Enums;

namespace PropertyService.Domain.Entities;

public class Unit : BaseEntity
{
  //  public Guid TenantId { get; set; }
    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = default!;

    public string UnitNumber { get; set; } = default!;
    public string? Floor { get; set; }

    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }

    public UnitStatus Status { get; set; } = UnitStatus.Vacant;

    public List<UnitOccupancy> Occupancies { get; set; } = new();
    public List<ConditionLog> ConditionLogs { get; set; } = new();
}
