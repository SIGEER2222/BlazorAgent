using Bunit;
using MudBlazorLab.Components;
using Xunit;

public class HelloMudTests : BunitContext
{
  [Fact]
  public void Click_Increments_Count()
  {
    var cut = Render<HelloMud>();
    cut.Find("button").Click();
    Assert.Contains("计数：1", cut.Markup);
  }
}
