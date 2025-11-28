using global::InspectionSystem.Models;
using InspectionSystem.Services;
using MudBlazor;
using Microsoft.AspNetCore.Components;

namespace MudBlazorLab.Components.SystemModule.InspectionSystem.Components.IncomingInspection;

public partial class IncomingObjectsPanel : ComponentBase
{
    [Parameter] public bool Visible { get; set; }
    [Parameter] public InspectionForm? Form { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

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
    string _batchNo = string.Empty;
    long _totalQty = 0;
    decimal _samplingRatio = 0m;
    InspectionFormObject? _currentObject;

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
        _batchNo = string.Empty;
        _totalQty = 0;
        _samplingRatio = 0m;
        _dlgObject = true;
    }

    async Task SaveObject()
    {
        if (Form == null) { _dlgObject = false; return; }
        decimal? ratio = _samplingRatio;
        if (!ratio.HasValue || ratio == 0m)
        {
            var tpls = await ConfigSvc.GetTemplateObjectsAsync(Form.FormTemplateName);
            var objTpl = tpls.FirstOrDefault(x => x.ObjectName == _objectName);
            ratio = objTpl?.SamplingRatio;
        }
        await ObjectSvc.CreateObjectAsync(Form, _objectType, _objectName, string.IsNullOrWhiteSpace(_batchNo) ? null : _batchNo, _totalQty, ratio);
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
        }
    }

    void Close()
    {
        OnClose.InvokeAsync();
    }
}
