using Microsoft.Playwright;
using Xunit;

public class HelloPageE2E
{
  [Fact]
  public async Task Click_Increments()
  {
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
    var page = await browser.NewPageAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    await page.GotoAsync($"{baseUrl}/hello");
    await page.GetByRole(AriaRole.Button, new() { Name = "增加" }).ClickAsync();
    await Microsoft.Playwright.Assertions.Expect(page.Locator("text=计数：1")).ToBeVisibleAsync();
  }

  [Fact]
  public async Task Api_Login_Admin_Returns_OK()
  {
    using var playwright = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await playwright.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    var res = await api.PostAsync("/auth/login", new() { DataObject = new { Username = "admin@example.com", Password = "P@ssw0rd!" } });
    Assert.True(res.Ok);
  }

  [Fact]
  public async Task Api_Login_Wrong_Returns_Unauthorized()
  {
    using var playwright = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await playwright.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    var res = await api.PostAsync("/auth/login", new() { DataObject = new { Username = "admin@example.com", Password = "wrong" } });
    Assert.Equal(401, res.Status);
  }

  [Fact]
  public async Task Api_Logout_Returns_OK()
  {
    using var playwright = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await playwright.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    var res = await api.PostAsync("/auth/logout");
    Assert.True(res.Ok);
  }

  [Fact]
  public async Task Login_User_Access_Profile_Only()
  {
    using var playwright = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await playwright.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    await api.PostAsync("/auth/login", new() { DataObject = new { Username = "user@example.com", Password = "P@ssw0rd!" } });
    var storage = await api.StorageStateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
    await using var ctx = await browser.NewContextAsync(new() { StorageState = storage });
    var page = await ctx.NewPageAsync();
    await page.GotoAsync($"{baseUrl}/profile");
    await Microsoft.Playwright.Assertions.Expect(page.Locator("text=用户信息")).ToBeVisibleAsync();
    await page.GotoAsync($"{baseUrl}/reports");
    await Microsoft.Playwright.Assertions.Expect(page.Locator("text=报表页")).ToBeHiddenAsync();
    await page.GotoAsync($"{baseUrl}/editor");
    await Microsoft.Playwright.Assertions.Expect(page.Locator("text=编辑页")).ToBeHiddenAsync();
  }

  [Fact]
  public async Task Login_Manager_CanAccess_Reports()
  {
    using var playwright = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await playwright.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    await api.PostAsync("/auth/login", new() { DataObject = new { Username = "manager@example.com", Password = "P@ssw0rd!" } });
    var storage = await api.StorageStateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
    await using var ctx = await browser.NewContextAsync(new() { StorageState = storage });
    var page = await ctx.NewPageAsync();
    await page.GotoAsync($"{baseUrl}/reports");
    await Microsoft.Playwright.Assertions.Expect(page.Locator("text=报表页")).ToBeVisibleAsync();
  }

  [Fact]
  public async Task Login_Editor_CanAccess_Editor()
  {
    using var playwright = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await playwright.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    await api.PostAsync("/auth/login", new() { DataObject = new { Username = "editor@example.com", Password = "P@ssw0rd!" } });
    var storage = await api.StorageStateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
    await using var ctx = await browser.NewContextAsync(new() { StorageState = storage });
    var page = await ctx.NewPageAsync();
    await page.GotoAsync($"{baseUrl}/editor");
    await Microsoft.Playwright.Assertions.Expect(page.Locator("text=编辑页")).ToBeVisibleAsync();
  }

  [Fact]
  public async Task Ui_Login_User_Profile_Visible()
  {
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
    var page = await browser.NewPageAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    await page.GotoAsync($"{baseUrl}/login");
    await page.GetByLabel("用户名").FillAsync("user@example.com");
    await page.GetByLabel("密码").FillAsync("P@ssw0rd!");
    await page.GetByRole(AriaRole.Button, new() { Name = "登录" }).ClickAsync();
    await page.GotoAsync($"{baseUrl}/profile");
    await Microsoft.Playwright.Assertions.Expect(page.Locator("text=用户信息")).ToBeVisibleAsync();
  }

  [Fact]
  public async Task Ui_Login_Manager_Reports_Visible()
  {
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
    var page = await browser.NewPageAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    await page.GotoAsync($"{baseUrl}/login");
    await page.GetByLabel("用户名").FillAsync("manager@example.com");
    await page.GetByLabel("密码").FillAsync("P@ssw0rd!");
    await page.GetByRole(AriaRole.Button, new() { Name = "登录" }).ClickAsync();
    await page.GotoAsync($"{baseUrl}/reports");
    await Microsoft.Playwright.Assertions.Expect(page.Locator("text=报表页")).ToBeVisibleAsync();
  }
}
