using Microsoft.AspNetCore.Mvc;

namespace AnalyticsService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
}
