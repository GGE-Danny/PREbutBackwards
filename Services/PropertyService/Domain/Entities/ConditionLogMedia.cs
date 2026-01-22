using PropertyService.Domain.Common;

namespace PropertyService.Domain.Entities;

public class ConditionLogMedia : BaseEntity
{
  //  public Guid TenantId { get; set; }

    public Guid ConditionLogId { get; set; }
    public ConditionLog ConditionLog { get; set; } = default!;

    public string DocumentId { get; set; } = default!;
    public string? Caption { get; set; }
}
