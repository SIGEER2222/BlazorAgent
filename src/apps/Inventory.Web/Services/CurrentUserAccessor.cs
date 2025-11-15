using Inventory.Infrastructure.Services;

namespace Inventory.Web.Services;

public class CurrentUserAccessor : ICurrentUser
{
    readonly IHttpContextAccessor _http;
    public CurrentUserAccessor(IHttpContextAccessor http) { _http = http; }
    public string? Name => _http.HttpContext?.User?.Identity?.Name;
}