using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System.Linq;
using Xunit;

namespace Inventory.Unit;

public class DynamicEditDialogTests
{
  class M
  {
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
  }

  [Fact]
  public void SaveInvokesCallback()
  {
    using var ctx = new TestContext();
    ctx.Services.AddMudServices();
    ctx.JSInterop.SetupVoid("mudElementRef.addOnBlurEvent", _ => true);
    ctx.JSInterop.SetupVoid("mudPopover.initialize", _ => true);
    ctx.JSInterop.SetupVoid("mudPopover.connect", _ => true);
    ctx.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
    ctx.JSInterop.SetupVoid("mudPopoverHelper.countProviders", _ => true);
    var model = new M { Name = "A" };
    bool saved = false;
    var comp = ctx.RenderComponent<Inventory.Web.Components.Shared.DynamicEditDialog>(ps => ps
      .Add(p => p.RenderInline, true)
      .Add(p => p.Visible, true)
      .Add(p => p.Title, "测试")
      .Add(p => p.Model, model)
      .Add(p => p.Save, Microsoft.AspNetCore.Components.EventCallback.Factory.Create<object>(this, (object m) => { saved = true; }))
    );
    var btns = comp.FindAll("button");
    var saveBtn = btns.FirstOrDefault(b => b.TextContent.Contains("保存"));
    Assert.NotNull(saveBtn);
    saveBtn!.Click();
    Assert.True(saved);
  }
}