using global::InspectionSystem.Models;
using InspectionSystem.Services;
using HmiInspection.Models;
using MudBlazor;
using Microsoft.AspNetCore.Components;

namespace MudBlazorLab.Components.Components.InspectionPanel;

public partial class ObjectsPanel : ComponentBase
{
    [Parameter] public bool Visible { get; set; }
    [Parameter] public InspectionForm? Form { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public FormTable FormType { get; set; }

    [Inject] IInspectionConfigService ConfigSvc { get; set; }
    [Inject] IInspectionObjectService ObjectSvc { get; set; }

    MudDataGrid<InspectionObjectView> InspectionObjectGrid;
    List<InspectionFormObject> _objects = new();
    List<InspectionObjectView> _objectsView = new();
    int _objPageSize = 10;
    int[] _objPageOptions = new[] { 10, 20, 50 };
    bool _dlgObject;
    string _objectType = string.Empty;
    string _objectName = string.Empty;
    string _objectCreator = string.Empty;
    DateTime? _objectCreatedAt = DateTime.Now;
    InspectionFormObject? _currentObject;
    bool _openDetailsPanel;
    bool AllowAddDetail => FormType != FormTable.开班点检单 || Form != null || Form.Status == "开始";

    protected override async Task OnParametersSetAsync()
    {
        if (Visible && Form != null)
        {
            await ReloadObjects();
        }
    }

    async Task ReloadObjects()
    {
        _objects = await ObjectSvc.GetObjectsAsync(Form!.Sysid);
        _objectsView = await ObjectSvc.GetObjectViewsAsync(Form!);
        _currentObject = _objects.FirstOrDefault();
    }

    void NewObject()
    {
        if (Form == null) return;
        _objectType = string.Empty;
        _objectName = string.Empty;
        _objectCreator = string.Empty;
        _objectCreatedAt = DateTime.Now;
        _dlgObject = true;
    }

    async Task SaveObject()
    {
        if (Form == null) { _dlgObject = false; return; }
        await ObjectSvc.CreateObjectAsync(Form, _objectType, _objectName);
        _dlgObject = false;
        await ReloadObjects();
    }

    async Task<IEnumerable<string>> SearchObjectTypes(string value, CancellationToken _)
    {
        if (Form == null) return Enumerable.Empty<string>();
        return await Filter(await ConfigSvc.GetObjectTypesAsync(Form.FormTemplateName), value);
    }

    static Task<IEnumerable<string>> Filter(IEnumerable<string> src, string value)
    {
        var v = value ?? string.Empty;
        var items = src.Where(x => string.IsNullOrEmpty(v) || x.Contains(v, StringComparison.OrdinalIgnoreCase)).Take(10).ToList();
        return Task.FromResult(items.AsEnumerable());
    }

    void OnObjectRowClickView(DataGridRowClickEventArgs<InspectionObjectView> e)
    {
        var obj = _objects.FirstOrDefault(o => o.Sysid == e.Item.Sysid);
        if (obj != null)
        {
            _currentObject = obj;
            _openDetailsPanel = true;
        }
    }

    void Close()
    {
        OnClose.InvokeAsync();
    }

    void CloseDetailsPanel()
    {
        _openDetailsPanel = false;
    }
}

