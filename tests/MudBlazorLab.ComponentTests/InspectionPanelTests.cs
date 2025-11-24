using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudBlazor;
using MudBlazorLab.Components.Components.InspectionPanel;
using MudBlazorLab.Components.Services;
using MudBlazorLab.Components.Models;
using InspectionSystem.Services;

public class InspectionPanelTests : BunitContext
{
  public InspectionPanelTests()
  {
    Services.AddMudServices();
    Services.AddSingleton<IInspectionConfigService, InMemoryInspectionConfigService>();
    Services.AddSingleton<IInspectionDataService, InMemoryInspectionDataService>();
    JSInterop.Mode = JSRuntimeMode.Loose;
    JSInterop.SetupVoid("mudElementRef.addOnBlurEvent", _ => true);
    JSInterop.SetupVoid("mudPopover.initialize", _ => true);
    JSInterop.SetupVoid("mudPopover.connect", _ => true);
    JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
    JSInterop.SetupVoid("mudPopoverHelper.countProviders", _ => true);
    JSInterop.SetupVoid("mudInput.initialize", _ => true);
    JSInterop.SetupVoid("mudAutocomplete.initialize", _ => true);
    JSInterop.SetupVoid("mudAutocomplete.keydown", _ => true);
    JSInterop.SetupVoid("mudAutocomplete.blur", _ => true);
    JSInterop.SetupVoid("mudDatePicker.initialize", _ => true);
    JSInterop.SetupVoid("mudDatePicker.toggleOpen", _ => true);
  }

  [Fact]
  public void CreateDoc_Required_Disables_Submit_Until_Filled()
  {
    var cut = Render(builder =>
    {
      builder.OpenComponent(0, typeof(MudPopoverProvider));
      builder.CloseComponent();
      builder.OpenComponent(1, typeof(MudDialogProvider));
      builder.CloseComponent();
      builder.OpenComponent(2, typeof(InspectionPanel));
      builder.CloseComponent();
    });
    var addBtn = cut.FindAll("button").First(b => b.TextContent.Contains("新增表单"));
    addBtn.Click();
    var okBtn = cut.FindAll("button").First(b => b.TextContent.Contains("确定"));
    Assert.True(okBtn.HasAttribute("disabled"));
    var inputs = cut.FindAll("input");
    inputs.First(i => i.GetAttribute("aria-label") == "模板").Change("设备点检");
    inputs.First(i => i.GetAttribute("aria-label") == "产线").Change("Line-X");
    inputs.First(i => i.GetAttribute("aria-label") == "工单").Change("WO-999");
    inputs.First(i => i.GetAttribute("aria-label") == "单号").Change("DOC-001");
    inputs.First(i => i.GetAttribute("aria-label") == "创建人").Change("测试者");
    okBtn = cut.FindAll("button").First(b => b.TextContent.Contains("确定"));
    Assert.False(okBtn.HasAttribute("disabled"));
    okBtn.Click();
    Assert.Contains("DOC-001", cut.Markup);
    Assert.Contains("Line-X", cut.Markup);
  }

  [Fact]
  public void Add_Bulk_Details_For_Object_Only_Numeric_And_Result()
  {
    var data = Services.GetRequiredService<IInspectionDataService>();
    var doc = new InspectionSystem.Models.InspectionDoc { TemplateName = "设备点检", DocNumber = "DOC-TEST", ProductionLineName = "Line-X", WorkOrderNumbers = "WO-999", Creator = "测试者", CreatedAt = DateTime.Now, Status = InspectionSystem.Models.InspectionStatus.创建 };
    data.CreateDocAsync(doc).GetAwaiter().GetResult();
    data.UpdateDocStatusAsync(doc.DocNumber, InspectionSystem.Models.InspectionStatus.开始).GetAwaiter().GetResult();
    var obj = new InspectionSystem.Models.InspectionObject { TemplateName = doc.TemplateName, DocNumber = doc.DocNumber, ObjectType = "设备", ObjectName = "OBJ-1", Creator = doc.Creator, CreatedAt = DateTime.Now };
    data.AddObjectAsync(obj).GetAwaiter().GetResult();

    var cut = Render(builder =>
    {
      builder.OpenComponent(0, typeof(MudPopoverProvider));
      builder.CloseComponent();
      builder.OpenComponent(1, typeof(MudDialogProvider));
      builder.CloseComponent();
      builder.OpenComponent(2, typeof(InspectionPanel));
      builder.CloseComponent();
    });
    var addDetailBtns = cut.FindAll("button").Where(b => b.TextContent.Contains("添加详情项")).ToList();
    Assert.True(addDetailBtns.Any());
    addDetailBtns.First().Click();
    var numberInputs = cut.FindAll("input").Where(i => (i.GetAttribute("type") ?? "").Contains("number")).ToList();
    Assert.True(numberInputs.Any());
    numberInputs.First().Change("1");
    var confirm = cut.FindAll("button").First(b => b.TextContent.Contains("确定"));
    confirm.Click();
    Assert.Contains("详情项", cut.Markup);
  }
}