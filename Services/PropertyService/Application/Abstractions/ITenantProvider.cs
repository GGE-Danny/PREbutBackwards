namespace PropertyService.Application.Abstractions;

public interface ITenantProvider
{
  //  Guid GetTenantId();
    string? GetUserId();
}
