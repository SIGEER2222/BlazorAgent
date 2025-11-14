namespace MudBlazorLab.Components.Services;

public static class AuthPolicies
{
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireManagerOrAdmin = "RequireManagerOrAdmin";
    public const string RequireEditor = "RequireEditor";
}
