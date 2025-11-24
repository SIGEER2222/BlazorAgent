using Microsoft.Playwright;
using Xunit;

public class InspectionPanelE2E
{
  [Fact]
  public async Task Bulk_Add_Details_For_Object()
  {
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
    var page = await browser.NewPageAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    await page.GotoAsync($"{baseUrl}/shift-random-inspection");
    await Microsoft.Playwright.Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "新增表单" })).ToBeVisibleAsync();
    await Microsoft.Playwright.Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "添加详情项" })).ToBeVisibleAsync();
  }
}