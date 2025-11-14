using System.Security.Claims;

namespace MudBlazorLab.Components.Services;

public interface IPermissionService
{
    bool HasAccess(ClaimsPrincipal? user, string feature);
}

public static class PermissionRegistry
{
    public static readonly Dictionary<string, string[]> FeatureRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Admin"] = new[] { "Admin" },
        ["Reports"] = new[] { "Manager", "Admin" },
        ["Editor"] = new[] { "Editor" },
        ["Profile"] = new[] { "User", "Manager", "Admin", "Editor" },
        ["IncomingInspection"] = new[] { "Manager", "Admin" },
        ["ProcessInspection"] = new[] { "Manager", "Admin" },
        ["ShippingInspection"] = new[] { "Manager", "Admin" },
        ["ShiftInspection"] = new[] { "User", "Manager", "Admin" },
        ["RandomInspection"] = new[] { "User", "Manager", "Admin" },
        ["MasterData"] = new[] { "Editor", "Manager", "Admin" },
        ["Purchase"] = new[] { "Manager", "Admin" },
        ["Sales"] = new[] { "Manager", "Admin" },
        ["Inventory"] = new[] { "Manager", "Admin" }
    };
}

public class PermissionService : IPermissionService
{
    readonly Dictionary<string, string[]> _roles;

    public PermissionService(Dictionary<string, string[]> roles)
    {
        _roles = roles;
    }

    public bool HasAccess(ClaimsPrincipal? user, string feature)
    {
        if (!_roles.TryGetValue(feature, out var rs)) return true;
        if (user == null || !user.Identity?.IsAuthenticated == true) return false;
        foreach (var r in rs)
        {
            if (user.IsInRole(r)) return true;
        }
        return false;
    }
}

