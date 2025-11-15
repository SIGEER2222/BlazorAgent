using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Inventory.E2E;

public class AuthAndProductsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
  readonly WebApplicationFactory<Program> _factory;
  public AuthAndProductsApiTests(WebApplicationFactory<Program> factory)
  {
    _factory = factory.WithWebHostBuilder(builder => { });
  }

  [Fact]
  public async Task Login_Then_QueryProducts()
  {
    var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
    {
      AllowAutoRedirect = false,
      HandleCookies = true,
    });

    var loginResp = await client.PostAsJsonAsync("/auth/login", new { Username = "editor@example.com", Password = "P@ssw0rd!" });
    Assert.True(loginResp.IsSuccessStatusCode);

    var productsResp = await client.GetAsync("/api/products?page=1&pageSize=3");
    Assert.True(productsResp.IsSuccessStatusCode);
    var json = await productsResp.Content.ReadFromJsonAsync<ProductsPage>();
    Assert.NotNull(json);
    Assert.True(json!.total >= 0);
  }

  public record ProductsPage(object[] items, int total);
}
