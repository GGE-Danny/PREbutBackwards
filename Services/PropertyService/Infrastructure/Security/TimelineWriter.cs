using System.Text.Json;
using PropertyService.Application.Abstractions;
using PropertyService.Domain.Entities;
using PropertyService.Infrastructure.Persistence;

namespace PropertyService.Infrastructure.Services;

public interface ITimelineWriter
{
    Task WritePropertyEventAsync(Guid propertyId, string eventType, object? data = null);
}

public class TimelineWriter : ITimelineWriter
{
    private readonly PropertyDbContext _db;
    private readonly ITenantProvider _tenant;

    public TimelineWriter(PropertyDbContext db, ITenantProvider tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task WritePropertyEventAsync(Guid propertyId, string eventType, object? data = null)
    {
        _db.PropertyTimelineEvents.Add(new PropertyTimelineEvent
        {
          //  TenantId = _tenant.GetTenantId(),
            PropertyId = propertyId,
            EventType = eventType,
            ActorUserId = Guid.TryParse(_tenant.GetUserId(), out var userId) ? userId : (Guid?)null,
            DataJson = data is null ? null : JsonSerializer.Serialize(data),
            EventAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}
