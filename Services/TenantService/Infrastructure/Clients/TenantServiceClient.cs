using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace PropertyService.Infrastructure.Clients;

public interface ITenantServiceClient
{
    Task NotifyOccupancyAssignedAsync(object payload);
    Task NotifyOccupancyVacatedAsync(object payload);
}

public class TenantServiceClient : ITenantServiceClient
{
    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _http;

    public TenantServiceClient(IHttpClientFactory factory, IHttpContextAccessor http)
    {
        _factory = factory;
        _http = http;
    }

    private string? GetBearerToken()
    {
        var authHeader = _http.HttpContext?.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(authHeader)) return null;
        return authHeader; // already "Bearer xxx"
    }

    public async Task NotifyOccupancyAssignedAsync(object payload)
    {
        var client = _factory.CreateClient("TenantService");
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/internal/occupancy-assigned");

        var bearer = GetBearerToken();
        if (!string.IsNullOrWhiteSpace(bearer))
            req.Headers.Authorization = AuthenticationHeaderValue.Parse(bearer);

        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var res = await client.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }

    public async Task NotifyOccupancyVacatedAsync(object payload)
    {
        var client = _factory.CreateClient("TenantService");
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/internal/occupancy-vacated");

        var bearer = GetBearerToken();
        if (!string.IsNullOrWhiteSpace(bearer))
            req.Headers.Authorization = AuthenticationHeaderValue.Parse(bearer);

        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var res = await client.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }
}
