using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PropertyService.Application.Abstractions;

namespace PropertyService.Infrastructure.Security;

public class HttpTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _http;

    public HttpTenantProvider(IHttpContextAccessor http) => _http = http;

    //public Guid GetTenantId()
    //{
    //    var ctx = _http.HttpContext;

    //    // 1) Prefer JWT claim (production path)
    //    var tenantIdStr =
    //        ctx?.User?.FindFirstValue("tenantId")
    //        ?? ctx?.User?.FindFirstValue("tenant_id");

    //    if (Guid.TryParse(tenantIdStr, out var tenantId))
    //        return tenantId;

    //    // 2) Dev/testing fallback: header
    //    var headerTenantId = ctx?.Request.Headers["X-Tenant-Id"].ToString();
    //    if (Guid.TryParse(headerTenantId, out var headerGuid))
    //        return headerGuid;

    //    // 3) Fail clearly
    //    throw new UnauthorizedAccessException(
    //        "Missing/invalid tenant id. Provide JWT claim 'tenantId/tenant_id' or header 'X-Tenant-Id'.");
    //}

    public string? GetUserId()
        => _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? _http.HttpContext?.User?.FindFirstValue("sub");
}
