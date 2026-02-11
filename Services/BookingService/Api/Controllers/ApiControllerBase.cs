using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected bool TryGetCallerUserId(out Guid userId)
    {
        userId = Guid.Empty;

        var sub = User.FindFirstValue("sub");
        if (!string.IsNullOrWhiteSpace(sub) && Guid.TryParse(sub, out userId))
            return true;

        var nameId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(nameId) && Guid.TryParse(nameId, out userId))
            return true;

        return false;
    }

    protected bool IsTenant(ClaimsPrincipal user)
        => user.IsInRole("tenant"); // Role claim maps to ClaimTypes.Role

    protected bool CanManage(ClaimsPrincipal user)
        => user.IsInRole("super_admin")
           || user.IsInRole("manager")
           || user.IsInRole("support")
           || user.IsInRole("sales");

    protected bool CanAccessTenant(Guid tenantUserId)
    {
        if (!TryGetCallerUserId(out var callerId))
            return false;

        if (IsTenant(User))
            return tenantUserId == callerId;

        return CanManage(User);
    }
}
