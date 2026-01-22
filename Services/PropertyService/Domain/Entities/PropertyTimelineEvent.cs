using PropertyService.Domain.Common;

namespace PropertyService.Domain.Entities;

public class PropertyTimelineEvent : BaseEntity
{
  //  public Guid TenantId { get; set; }

    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = default!;

    public string EventType { get; set; } = default!;
    public DateTime EventAt { get; set; } = DateTime.UtcNow;

    public Guid? ActorUserId { get; set; }

    public string? DataJson { get; set; } // keep payload lightweight
}
