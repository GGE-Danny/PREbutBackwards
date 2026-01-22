using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace PropertyService.Infrastructure.Clients;

public class TenantServiceClient : ITenantServiceClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _ctx;

    public TenantServiceClient(IHttpClientFactory factory, IHttpContextAccessor ctx)
    {
        _http = factory.CreateClient("TenantService");
        _ctx = ctx;
    }

    private void CopyAuthHeader()
    {
        var auth = _ctx.HttpContext?.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrWhiteSpace(auth))
            _http.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(auth);
    }

    public async Task NotifyOccupancyAssignedAsync(Guid tenantUserId, Guid propertyId, Guid unitId, DateOnly startDate)
    {
        CopyAuthHeader();

        var payload = new { tenantUserId, propertyId, unitId, startDate };
        var res = await _http.PostAsJsonAsync("/api/v1/internal/occupancy-assigned", payload);
        res.EnsureSuccessStatusCode();
    }

    public async Task NotifyOccupancyVacatedAsync(Guid tenantUserId, Guid propertyId, Guid unitId, DateOnly endDate)
    {
        CopyAuthHeader();

        var payload = new { tenantUserId, propertyId, unitId, endDate };
        var res = await _http.PostAsJsonAsync("/api/v1/internal/occupancy-vacated", payload);
        res.EnsureSuccessStatusCode();
    }
}
