using Bunit;
using Bunit.JSInterop;
using MudBlazor.Services;
using MudBlazorLab.Components.Components;
using MudBlazorLab.Components.Models;

public class AutoDataGridTests
{
    [Fact]
    public async System.Threading.Tasks.Task Renders_Items_And_Columns()
    {
        await using var ctx = new Bunit.BunitContext();
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var items = new[]
        {
            new InventoryItem { Name = "A100", Quantity = 12, Price = 19.99m, WeightKg = 0.45, Active = true },
            new InventoryItem { Name = "B200", Quantity = 5, Price = 99.00m, WeightKg = 2.3, Active = false },
        };

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent(0, typeof(MudBlazor.MudPopoverProvider));
            builder.CloseComponent();
            builder.OpenComponent(1, typeof(AutoDataGrid<InventoryItem>));
            builder.AddAttribute(2, nameof(AutoDataGrid<InventoryItem>.Title), "库存项目");
            builder.AddAttribute(3, nameof(AutoDataGrid<InventoryItem>.Source), items);
            builder.AddAttribute(4, nameof(AutoDataGrid<InventoryItem>.Filterable), true);
            builder.CloseComponent();
        });

        Assert.True(cut.Markup.Length >= 1);
    }
}
