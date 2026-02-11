using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace BookingService.Infrastructure.Clients;

public sealed class PropertyServiceClient : IPropertyServiceClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _ctx;

    public PropertyServiceClient(HttpClient http, IHttpContextAccessor ctx)
    {
        _http = http;
        _ctx = ctx;
    }

    public async Task<bool> UnitBelongsToPropertyAsync(Guid propertyId, Guid unitId, CancellationToken ct = default)
    {
        // Forward the incoming Authorization header
        var auth = _ctx.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(auth))
        {
            _http.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(auth);
        }

        // Example endpoint (match your PropertyService actual route)
        // GET /api/v1/properties/{propertyId}/units/{unitId}/belongs
        var resp = await _http.GetAsync($"/api/v1/properties/{propertyId}/units/{unitId}/belongs", ct);

        if (!resp.IsSuccessStatusCode) return false;

        var content = await resp.Content.ReadAsStringAsync(ct);
        return bool.TryParse(content, out var result) && result;
    }
}
