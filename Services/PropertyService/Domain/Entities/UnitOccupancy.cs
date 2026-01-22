using PropertyService.Domain.Common;

namespace PropertyService.Domain.Entities;

public class UnitOccupancy : BaseEntity
{
  //  public Guid TenantId { get; set; }

    public Guid UnitId { get; set; }
    public Unit Unit { get; set; } = default!;

    public Guid TenantUserId { get; set; }
    // tenant service reference
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; } // null = current
    public string? Notes { get; set; }
}
