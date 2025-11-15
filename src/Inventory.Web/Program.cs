using MudBlazor.Services;
using MudBlazorLab.Components.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Inventory.Web.Endpoints;
using Inventory.Web.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IPermissionService>(new PermissionService(PermissionRegistry.FeatureRoles));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.RequireAdmin, policy => policy.RequireRole("Admin"));
    options.AddPolicy(AuthPolicies.RequireManagerOrAdmin, policy => policy.RequireRole("Manager", "Admin"));
    options.AddPolicy(AuthPolicies.RequireEditor, policy => policy.RequireRole("Editor"));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddInventoryServices(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  app.UseHsts();
}

var httpsPort = app.Configuration["ASPNETCORE_HTTPS_PORT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT");
if (!string.IsNullOrEmpty(httpsPort))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.Use(async (ctx, next) =>
{
    if (!ctx.Request.Path.StartsWithSegments("/api") && !ctx.Request.Path.StartsWithSegments("/admin") && !ctx.Request.Path.StartsWithSegments("/auth"))
    {
        await next();
        return;
    }
    try
    {
        await next();
    }
    catch (BadHttpRequestException ex)
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        ctx.Response.ContentType = "application/json";
        var payload = System.Text.Json.JsonSerializer.Serialize(new { error = "BadRequest", message = ex.Message, traceId = ctx.TraceIdentifier, path = ctx.Request.Path.Value });
        await ctx.Response.WriteAsync(payload);
    }
    catch (SqlSugar.SqlSugarException ex)
    {
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        ctx.Response.ContentType = "application/json";
        var payload = System.Text.Json.JsonSerializer.Serialize(new { error = "DatabaseError", message = ex.Message, traceId = ctx.TraceIdentifier, path = ctx.Request.Path.Value });
        await ctx.Response.WriteAsync(payload);
    }
    catch (Exception ex)
    {
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        ctx.Response.ContentType = "application/json";
        var payload = System.Text.Json.JsonSerializer.Serialize(new { error = "ServerError", message = ex.Message, traceId = ctx.TraceIdentifier, path = ctx.Request.Path.Value });
        await ctx.Response.WriteAsync(payload);
    }
});

app.MapStaticAssets();
app.MapRazorComponents<Inventory.Web.Components.App>()
    .AddInteractiveServerRenderMode();
app.MapAuth();
app.MapReports();
Inventory.Web.Endpoints.MasterDataEndpoints.MapMasterData(app);
Inventory.Web.Endpoints.ProcessEndpoints.MapProcess(app);
if (app.Environment.IsDevelopment())
{
    Inventory.Web.Endpoints.E2ESeedEndpoints.Map(app);
}
var dbForSeed = app.Services.GetRequiredService<Inventory.Infrastructure.Data.InventoryDb>();
Inventory.Web.Data.SeedData.EnsureDemo(dbForSeed);

app.Run();

public partial class Program { }
