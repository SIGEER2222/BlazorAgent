using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Bunit.JSInterop;
using System.Security.Claims;
using Inventory.Web.Components.Pages;

public class LoginTests : BunitContext
{
  public LoginTests()
  {
    Services.AddMudServices();
    Services.AddAuthorizationCore();
    Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationService, AllowAllAuthorizationService>();
    JSInterop.Mode = JSRuntimeMode.Loose;
  }

  [Fact]
  public void Login_Render_Shows_Title_And_Fields()
  {
    var cut = Render<Login>();
    Assert.Contains("欢迎登录", cut.Markup);
    Assert.Contains("用户名", cut.Markup);
    Assert.Contains("密码", cut.Markup);
    Assert.Contains("登录", cut.Markup);
  }

  [Fact]
  public void Login_Query_Error_Shows_Error_Message()
  {
    var nav = Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    nav.NavigateTo("/login?error=1");
    var cut = Render<Login>();
    Assert.Contains("用户名或密码错误", cut.Markup);
  }
}

class AllowAllAuthorizationService : Microsoft.AspNetCore.Authorization.IAuthorizationService
{
  public Task<Microsoft.AspNetCore.Authorization.AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<Microsoft.AspNetCore.Authorization.IAuthorizationRequirement> requirements)
    => Task.FromResult(Microsoft.AspNetCore.Authorization.AuthorizationResult.Success());

  public Task<Microsoft.AspNetCore.Authorization.AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
    => Task.FromResult(Microsoft.AspNetCore.Authorization.AuthorizationResult.Success());
}