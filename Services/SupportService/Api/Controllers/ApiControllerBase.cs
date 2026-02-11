using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SupportService.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected bool TryGetCallerUserId(out Guid userId)
    {
        userId = Guid.Empty;

        var sub = User.FindFirstValue("sub")
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(sub, out userId);
    }

    protected bool IsTenant(ClaimsPrincipal user)
    {
        var role = user.FindFirstValue(ClaimTypes.Role);
        return role == "tenant";
    }

    protected bool CanManage(ClaimsPrincipal user)
    {
        var role = user.FindFirstValue(ClaimTypes.Role);
        return role is "super_admin" or "manager" or "support" or "sales";
    }

    protected bool CanAccessTenant(Guid tenantUserId)
    {
        if (CanManage(User))
            return true;

        if (!TryGetCallerUserId(out var callerId))
            return false;

        return callerId == tenantUserId;
    }
}
