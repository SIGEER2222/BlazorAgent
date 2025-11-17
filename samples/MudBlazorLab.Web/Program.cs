using MudBlazor.Services;
using MudBlazorLab.Web.Components;
using MudBlazorLab.Components.Services;
using MudBlazorLab.Components.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using MudBlazorLab.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IPermissionService>(new PermissionService(PermissionRegistry.FeatureRoles));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
    });

builder.Services.AddAuthorization(options => {
    options.AddPolicy(AuthPolicies.RequireAdmin, policy => policy.RequireRole("Admin"));
    options.AddPolicy(AuthPolicies.RequireManagerOrAdmin, policy => policy.RequireRole("Manager", "Admin"));
    options.AddPolicy(AuthPolicies.RequireEditor, policy => policy.RequireRole("Editor"));
});

builder.Services.AddHttpContextAccessor();

// Register RabbitMQ Consumer Service and host it
builder.Services.AddSingleton<RabbitMQConsumerService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<RabbitMQConsumerService>());

// Register RabbitMQ Message Service for UI components
builder.Services.AddSingleton<IRabbitMQMessageService>(sp => 
    new RabbitMQMessageService(sp.GetRequiredService<RabbitMQConsumerService>()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var httpsPort = app.Configuration["ASPNETCORE_HTTPS_PORT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT");
if (!string.IsNullOrEmpty(httpsPort)) {
    app.UseHttpsRedirection();
}


app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/auth/login", async (HttpContext ctx, LoginDto dto) => {
    var principal = UserService.SignIn(dto.Username, dto.Password);
    if (principal == null) return Results.Unauthorized();
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    return Results.Ok(new { name = principal.Identity!.Name });
}).DisableAntiforgery();

app.MapPost("/auth/login-form", async (HttpContext ctx) => {
    var form = await ctx.Request.ReadFormAsync();
    var username = form["Username"].ToString();
    var password = form["Password"].ToString();
    var principal = UserService.SignIn(username, password);
    if (principal == null) return Results.Redirect("/login?error=1");
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    return Results.Redirect("/");
}).DisableAntiforgery();

app.MapPost("/auth/logout", async (HttpContext ctx) => {
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
}).DisableAntiforgery();

app.Run();
