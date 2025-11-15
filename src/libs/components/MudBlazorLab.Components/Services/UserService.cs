using System.Security.Claims;

namespace MudBlazorLab.Components.Services;

public static class UserService
{
    static readonly Dictionary<string, (string Password, string Role)> Users = new()
    {
        ["admin@example.com"] = ("P@ssw0rd!", "Admin"),
        ["user@example.com"] = ("P@ssw0rd!", "User"),
        ["manager@example.com"] = ("P@ssw0rd!", "Manager"),
        ["editor@example.com"] = ("P@ssw0rd!", "Editor"),
    };

    public static ClaimsPrincipal? SignIn(string username, string password)
    {
        if (!Users.TryGetValue(username, out var u)) return null;
        if (u.Password != password) return null;
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, u.Role)
        };
        var identity = new ClaimsIdentity(claims, "cookie");
        return new ClaimsPrincipal(identity);
    }
}
