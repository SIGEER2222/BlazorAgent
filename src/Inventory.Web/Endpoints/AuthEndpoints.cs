using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Inventory.Web.Endpoints;

  public static class AuthEndpoints
  {
    public static void MapAuth(this IEndpointRouteBuilder app)
    {
      app.MapPost("/auth/login", async (HttpContext ctx, Inventory.Web.Models.LoginDto dto) =>
    {
      var principal = MudBlazorLab.Components.Services.UserService.SignIn(dto.Username, dto.Password);
      if (principal == null) return Results.Unauthorized();
      await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
      return Results.Ok(new { name = principal.Identity!.Name });
    }).DisableAntiforgery();

    app.MapPost("/auth/login-form", async (HttpContext ctx) =>
    {
      var form = await ctx.Request.ReadFormAsync();
      var username = form["Username"].ToString();
      var password = form["Password"].ToString();
      var principal = MudBlazorLab.Components.Services.UserService.SignIn(username, password);
      if (principal == null) return Results.Redirect("/login?error=1");
      await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
      return Results.Redirect("/");
    }).DisableAntiforgery();

    app.MapPost("/auth/logout", async (HttpContext ctx) =>
    {
      await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
      return Results.Ok();
    }).DisableAntiforgery();

      app.MapGet("/health", () => Results.Ok(new { status = "OK" }));

      app.MapGet("/admin/permissions", (MudBlazorLab.Components.Services.IPermissionService ps) =>
      {
        return Results.Ok(MudBlazorLab.Components.Services.PermissionRegistry.FeatureRoles);
      }).RequireAuthorization(MudBlazorLab.Components.Services.AuthPolicies.RequireAdmin);

      app.MapPut("/admin/permissions/{feature}", async (string feature, string[] roles, Inventory.Infrastructure.Services.AuditService audit) =>
      {
        MudBlazorLab.Components.Services.PermissionRegistry.FeatureRoles[feature] = roles;
        await audit.LogAsync("Admin.Permission.Update", "Permission", feature, new { roles });
        return Results.NoContent();
      }).RequireAuthorization(MudBlazorLab.Components.Services.AuthPolicies.RequireAdmin).DisableAntiforgery();

      app.MapPost("/admin/permissions/reset", async (Inventory.Infrastructure.Services.AuditService audit) =>
      {
        var defaults = MudBlazorLab.Components.Services.PermissionRegistry.FeatureRoles;
        MudBlazorLab.Components.Services.PermissionRegistry.FeatureRoles.Clear();
        foreach (var kv in defaults.ToList())
          MudBlazorLab.Components.Services.PermissionRegistry.FeatureRoles[kv.Key] = kv.Value;
        await audit.LogAsync("Admin.Permission.Reset", "Permission", null, null);
        return Results.NoContent();
      }).RequireAuthorization(MudBlazorLab.Components.Services.AuthPolicies.RequireAdmin).DisableAntiforgery();

      app.MapGet("/permissions/check", (HttpContext ctx, MudBlazorLab.Components.Services.IPermissionService ps, string feature) =>
      {
        var ok = ps.HasAccess(ctx.User, feature);
        return Results.Ok(new { feature, hasAccess = ok });
      }).RequireAuthorization();
    }
  }