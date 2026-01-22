using PropertyService.Domain.Common;

namespace PropertyService.Domain.Entities;

public class ConditionLog : BaseEntity
{
  //  public Guid TenantId { get; set; }

    public Guid UnitId { get; set; }
    public Unit Unit { get; set; } = default!;

    public string LogType { get; set; } = "PreRental"; // PreRental/Inspection/PostRental/Maintenance
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public Guid? CreatedByUserId { get; set; }

    public List<ConditionLogMedia> Media { get; set; } = new();
}
