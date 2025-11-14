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
}
