using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace TenantService.Infrastructure.Clients;

public class PropertyServiceClient : IPropertyServiceClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _ctx;

    public PropertyServiceClient(IHttpClientFactory factory, IHttpContextAccessor ctx)
    {
        _http = factory.CreateClient("PropertyService");
        _ctx = ctx;
    }

    private void CopyAuthHeader()
    {
        var auth = _ctx.HttpContext?.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrWhiteSpace(auth))
            _http.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(auth);
    }

    public async Task<bool> UnitBelongsToPropertyAsync(Guid propertyId, Guid unitId)
    {
        CopyAuthHeader();

        var res = await _http.GetAsync($"/api/v1/units/{unitId}");
        if (!res.IsSuccessStatusCode) return false;

        var json = await res.Content.ReadFromJsonAsync<UnitLookupResponse>();
        return json?.unit?.propertyId == propertyId;
    }

    private sealed class UnitLookupResponse { public UnitDto? unit { get; set; } }
    private sealed class UnitDto { public Guid propertyId { get; set; } }
}
