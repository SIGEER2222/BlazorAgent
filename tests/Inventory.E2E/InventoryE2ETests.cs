using Microsoft.Playwright;

public class InventoryE2ETests
{
  [Fact]
  public async Task Health_Returns_OK()
  {
    using var pw = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    var res = await api.GetAsync("/health");
    Assert.True(res.Ok);
  }

  [Fact]
  public async Task Login_UI_User_Sees_Home()
  {
    using var pw = await Playwright.CreateAsync();
    await using var browser = await pw.Chromium.LaunchAsync(new() { Headless = true });
    var page = await browser.NewPageAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    await page.GotoAsync($"{baseUrl}/login");
    await page.GetByLabel("用户名").FillAsync("user@example.com");
    await page.GetByLabel("密码").FillAsync("P@ssw0rd!");
    await page.GetByRole(AriaRole.Button, new() { Name = "登录" }).ClickAsync();
    await Microsoft.Playwright.Assertions.Expect(page.Locator("text=进销存系统")).ToBeVisibleAsync();
  }

  [Fact]
  public async Task Reports_Endpoints_Require_Auth()
  {
    using var pw = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });

    var unauth = await api.GetAsync("/reports/inventory/balance.xlsx");
    var ct = unauth.Headers.TryGetValue("content-type", out var v) ? v : string.Empty;
    Assert.DoesNotContain("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ct);

    var login = await api.PostAsync("/auth/login", new() { DataObject = new { Username = "admin@example.com", Password = "P@ssw0rd!" } });
    Assert.True(login.Ok);
    var storage = await api.StorageStateAsync();

    var api2 = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl, StorageState = storage });
    var res2 = await api2.GetAsync("/reports/inventory/balance.xlsx");
    Assert.True(res2.Ok);
    var ct2 = res2.Headers.TryGetValue("content-type", out var v2) ? v2 : string.Empty;
    Assert.Contains("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ct2);
  }

  [Fact]
  public async Task Reports_All_Unauthorized_NoExcel()
  {
    using var pw = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    var endpoints = new[] {
      "/reports/inventory/balance.xlsx",
      "/reports/inventory/movements.xlsx",
      "/reports/sales-summary.xlsx",
      "/reports/purchase-summary.xlsx",
      "/reports/sales-margin.xlsx",
      "/reports/audit.xlsx"
    };
    foreach (var ep in endpoints)
    {
      var res = await api.GetAsync(ep);
      var ct = res.Headers.TryGetValue("content-type", out var v) ? v : string.Empty;
      Assert.DoesNotContain("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ct);
    }
  }

  [Fact]
  public async Task Reports_All_Authorized_Download_Excel()
  {
    using var pw = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    await api.PostAsync("/auth/login", new() { DataObject = new { Username = "admin@example.com", Password = "P@ssw0rd!" } });
    var storage = await api.StorageStateAsync();
    var api2 = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl, StorageState = storage });
    var endpoints = new[] {
      "/reports/inventory/balance.xlsx",
      "/reports/inventory/movements.xlsx",
      "/reports/sales-summary.xlsx",
      "/reports/purchase-summary.xlsx",
      "/reports/sales-margin.xlsx",
      "/reports/audit.xlsx"
    };
    foreach (var ep in endpoints)
    {
      var res = await api2.GetAsync(ep);
      Assert.True(res.Ok);
      var ct = res.Headers.TryGetValue("content-type", out var v) ? v : string.Empty;
      Assert.Contains("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ct);
    }
  }

  [Fact]
  public async Task Products_Page_Visible_After_Login()
  {
    using var pw = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    await api.PostAsync("/auth/login", new() { DataObject = new { Username = "editor@example.com", Password = "P@ssw0rd!" } });
    var storage = await api.StorageStateAsync();
    await using var browser = await pw.Chromium.LaunchAsync(new() { Headless = true });
    await using var ctx = await browser.NewContextAsync(new() { StorageState = storage });
    var page = await ctx.NewPageAsync();
    await page.GotoAsync($"{baseUrl}/master-data/products");
    await Microsoft.Playwright.Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "产品资料" })).ToBeVisibleAsync();
    await Microsoft.Playwright.Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "新建" })).ToBeVisibleAsync();
  }

  [Fact]
  public async Task Products_Page_NotAuthorized_For_User()
  {
    using var pw = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    await api.PostAsync("/auth/login", new() { DataObject = new { Username = "user@example.com", Password = "P@ssw0rd!" } });
    var storage = await api.StorageStateAsync();
    await using var browser = await pw.Chromium.LaunchAsync(new() { Headless = true });
    await using var ctx = await browser.NewContextAsync(new() { StorageState = storage });
    var page = await ctx.NewPageAsync();
    await page.GotoAsync($"{baseUrl}/master-data/products");
    await Microsoft.Playwright.Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "产品资料" })).ToBeHiddenAsync();
  }

  [Fact]
  public async Task InventoryBalance_Page_NotAuthorized_For_User()
  {
    using var pw = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    await api.PostAsync("/auth/login", new() { DataObject = new { Username = "user@example.com", Password = "P@ssw0rd!" } });
    var storage = await api.StorageStateAsync();
    await using var browser = await pw.Chromium.LaunchAsync(new() { Headless = true });
    await using var ctx = await browser.NewContextAsync(new() { StorageState = storage });
    var page = await ctx.NewPageAsync();
    await page.GotoAsync($"{baseUrl}/inventory/balance");
    await Microsoft.Playwright.Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "库存余额" })).ToBeHiddenAsync();
  }

  [Fact]
  public async Task Reports_SalesSummary_Authorized_After_Login()
  {
    using var pw = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    await api.PostAsync("/auth/login", new() { DataObject = new { Username = "admin@example.com", Password = "P@ssw0rd!" } });
    var storage = await api.StorageStateAsync();
    var api2 = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl, StorageState = storage });
    var res = await api2.GetAsync("/reports/sales-summary.xlsx");
    Assert.True(res.Ok);
    var ct = res.Headers.TryGetValue("content-type", out var v) ? v : string.Empty;
    Assert.Contains("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ct);
  }

  [Fact]
  public async Task Logout_Then_Reports_NoExcel()
  {
    using var pw = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    await api.PostAsync("/auth/login", new() { DataObject = new { Username = "editor@example.com", Password = "P@ssw0rd!" } });
    await api.PostAsync("/auth/logout");
    var res = await api.GetAsync("/reports/purchase-summary.xlsx");
    var ct = res.Headers.TryGetValue("content-type", out var v) ? v : string.Empty;
    Assert.DoesNotContain("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ct);
  }

  [Fact]
  public async Task Api_Login_Wrong_Unauthorized()
  {
    using var pw = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    var res = await api.PostAsync("/auth/login", new() { DataObject = new { Username = "admin@example.com", Password = "wrong" } });
    Assert.Equal(401, res.Status);
  }

  [Fact]
  public async Task Purchase_Order_Flow_Works()
  {
    using var pw = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    await api.PostAsync("/auth/login", new() { DataObject = new { Username = "manager@example.com", Password = "P@ssw0rd!" } });
    var storage = await api.StorageStateAsync();
    await using var browser = await pw.Chromium.LaunchAsync(new() { Headless = true });
    await using var ctx = await browser.NewContextAsync(new() { StorageState = storage });
    var page = await ctx.NewPageAsync();
    await page.GotoAsync($"{baseUrl}/purchase/orders");
    await Microsoft.Playwright.Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "采购订单" })).ToBeVisibleAsync();
    await page.GetByRole(AriaRole.Button, new() { Name = "新建订单" }).ClickAsync();
    await Microsoft.Playwright.Assertions.Expect(page.GetByPlaceholder("订单号")).ToBeVisibleAsync();
    await page.GetByPlaceholder("订单号").FillAsync($"PO-{DateTime.UtcNow.Ticks}");
    await page.GetByPlaceholder("供应商代码").FillAsync("SUP01");
    await page.GetByRole(AriaRole.Button, new() { Name = "保存" }).ClickAsync();
    await page.GetByTitle("加行").ClickAsync();
    await Microsoft.Playwright.Assertions.Expect(page.GetByPlaceholder("产品代码")).ToBeVisibleAsync();
    await page.GetByPlaceholder("产品代码").FillAsync("P002");
    await page.Keyboard.PressAsync("Enter");
    await page.GetByPlaceholder("仓库").FillAsync("WH1");
    await page.Keyboard.PressAsync("Enter");
    await page.GetByPlaceholder("数量").FillAsync("5");
    await page.GetByPlaceholder("单价").FillAsync("10");
    await page.GetByRole(AriaRole.Button, new() { Name = "保存" }).ClickAsync();
    await page.GetByTitle("审批").ClickAsync();
    await page.GetByTitle("收货入库").ClickAsync();
    var api2 = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl, StorageState = storage });
    var res = await api2.GetAsync("/reports/inventory/movements.xlsx");
    Assert.True(res.Ok);
  }

  [Fact]
  public async Task Sales_Order_Flow_Works()
  {
    using var pw = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    await api.PostAsync("/auth/login", new() { DataObject = new { Username = "manager@example.com", Password = "P@ssw0rd!" } });
    var storage = await api.StorageStateAsync();
    await using var browser = await pw.Chromium.LaunchAsync(new() { Headless = true });
    await using var ctx = await browser.NewContextAsync(new() { StorageState = storage });
    var page = await ctx.NewPageAsync();
    await page.GotoAsync($"{baseUrl}/sales/orders");
    await Microsoft.Playwright.Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "销售订单" })).ToBeVisibleAsync();
    await page.GetByRole(AriaRole.Button, new() { Name = "新建订单" }).ClickAsync();
    await Microsoft.Playwright.Assertions.Expect(page.GetByPlaceholder("订单号")).ToBeVisibleAsync();
    await page.GetByPlaceholder("订单号").FillAsync($"SO-{DateTime.UtcNow.Ticks}");
    await page.GetByPlaceholder("客户代码").FillAsync("CUST01");
    await page.GetByRole(AriaRole.Button, new() { Name = "保存" }).ClickAsync();
    await page.GetByTitle("加行").ClickAsync();
    await Microsoft.Playwright.Assertions.Expect(page.GetByPlaceholder("产品代码")).ToBeVisibleAsync();
    await page.GetByPlaceholder("产品代码").FillAsync("P002");
    await page.Keyboard.PressAsync("Enter");
    await page.GetByPlaceholder("仓库").FillAsync("WH1");
    await page.Keyboard.PressAsync("Enter");
    await page.GetByPlaceholder("数量").FillAsync("3");
    await page.GetByPlaceholder("单价").FillAsync("20");
    await page.GetByRole(AriaRole.Button, new() { Name = "保存" }).ClickAsync();
    await page.GetByTitle("审批").ClickAsync();
    await page.GetByTitle("发货出库").ClickAsync();
    var api2 = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl, StorageState = storage });
    var res = await api2.GetAsync("/reports/sales-margin.xlsx");
    Assert.True(res.Ok);
  }

  [Fact]
  public async Task Products_UI_Create_New()
  {
    using var pw = await Playwright.CreateAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    var api = await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
    await api.PostAsync("/auth/login", new() { DataObject = new { Username = "editor@example.com", Password = "P@ssw0rd!" } });
    var storage = await api.StorageStateAsync();
    await using var browser = await pw.Chromium.LaunchAsync(new() { Headless = true });
    await using var ctx = await browser.NewContextAsync(new() { StorageState = storage });
    var page = await ctx.NewPageAsync();
    await page.GotoAsync($"{baseUrl}/master-data/products");
    await Microsoft.Playwright.Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "产品资料" })).ToBeVisibleAsync();
    var code = $"PX-{DateTime.UtcNow.Ticks}";
    await page.GetByRole(AriaRole.Button, new() { Name = "新建" }).ClickAsync();
    await Microsoft.Playwright.Assertions.Expect(page.GetByPlaceholder("代码")).ToBeVisibleAsync();
    await page.GetByPlaceholder("代码").FillAsync(code);
    await page.GetByPlaceholder("名称").FillAsync("测试产品");
    await page.GetByRole(AriaRole.Button, new() { Name = "保存" }).ClickAsync();
    await page.GetByPlaceholder("搜索代码/名称").FillAsync(code);
    await Microsoft.Playwright.Assertions.Expect(page.Locator($"text={code}")).ToBeVisibleAsync();
  }

}