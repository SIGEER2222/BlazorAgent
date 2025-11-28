using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MudBlazor;
using Microsoft.AspNetCore.Components;

namespace MudBlazorLab.Components.SystemModule.InspectionSystem.Components;

public partial class GridDataGridSelect<TItem> where TItem : class
{
    HashSet<TItem> selectedItems = new();
    string selectedText = string.Empty;
    bool _open;

    [Parameter] public IEnumerable<TItem> Items { get; set; } = new List<TItem>();
    [Parameter] public RenderFragment ColumnsTemplate { get; set; }
    [Parameter] public EventCallback<HashSet<TItem>> SelectedItemsChanged { get; set; }
    [Parameter] public Func<TItem, string> DisplayTextSelector { get; set; }
    [Parameter] public string Label { get; set; } = "请选择";
    [Parameter] public bool FullWidth { get; set; } = true;
    [Parameter] public int MaxHeight { get; set; } = 600;
    [Parameter] public int GridMaxHeight { get; set; } = 500;
    [Parameter] public string GridHeight { get; set; } = "400";
    [Parameter] public bool Dense { get; set; } = true;
    [Parameter] public bool Filterable { get; set; } = true;
    [Parameter] public DataGridFilterMode FilterMode { get; set; } = DataGridFilterMode.ColumnFilterRow;
    [Parameter] public SortMode SortMode { get; set; } = SortMode.Multiple;
    [Parameter] public bool ShowPager { get; set; } = true;
    [Parameter] public Variant Variant { get; set; } = Variant.Text;
    [Parameter] public string Separator { get; set; } = ", ";

    [Parameter] public HashSet<TItem> SelectedItems
    {
        get => selectedItems;
        set
        {
            if (selectedItems != value)
            {
                selectedItems = value ?? new HashSet<TItem>();
                UpdateSelectedText();
            }
        }
    }

    async Task OnSelectedItemsChangedInternal(HashSet<TItem> items)
    {
        selectedItems = items ?? new HashSet<TItem>();
        UpdateSelectedText();
        await SelectedItemsChanged.InvokeAsync(selectedItems);
    }

    void UpdateSelectedText()
    {
        if (DisplayTextSelector == null)
        {
            selectedText = selectedItems.Any()
                ? $"已选择 {selectedItems.Count} 项"
                : string.Empty;
        }
        else
        {
            selectedText = selectedItems.Any()
                ? string.Join(Separator, selectedItems.Select(DisplayTextSelector))
                : string.Empty;
        }
    }

    protected override void OnParametersSet()
    {
        UpdateSelectedText();
    }
}
