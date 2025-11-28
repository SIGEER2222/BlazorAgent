using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InspectionSystem.Models;
using InspectionSystem.Services;
using HmiInspection.Models;
using MudBlazor;
using Microsoft.AspNetCore.Components;

namespace MudBlazorLab.Components.Components.InspectionPanel;

public partial class DetailsPanel : ComponentBase
{
    [Parameter] public bool Visible { get; set; }
    [Parameter] public InspectionForm? CurrentDoc { get; set; }
    [Parameter] public InspectionFormObject? CurrentObject { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    [Inject] IInspectionDetailService DetailSvc { get; set; }
    [Inject] IInspectionConfigService ConfigSvc { get; set; }

    List<InspectionFlatRow> _flatRows = new();
    int _detailPageSize = 15;
    int[] _detailPageOptions = new[] { 15, 30, 60 };
    bool _dlgDetail;
    Guid _targetObjectId;
    List<string> _configuredItems = new();
    List<string> _configuredUnits = new();
    List<string> _configuredDescriptions = new();
    List<InspectionEntryRow> _bulkDetails = new();
    readonly DialogOptions _dialogOptions = new() { FullWidth = true, MaxWidth = MaxWidth.ExtraLarge };
    string? _sampleBatchNo;
    bool _isRequiredSN;
    string _selectedSampleBatchNo = string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        if (Visible && CurrentObject != null)
        {
            _targetObjectId = CurrentObject.Sysid;
            await ReloadDetails();
            await LoadConfiguredItems();
        }
    }

    async Task ReloadDetails()
    {
        if (CurrentObject == null) return;
        _flatRows = await DetailSvc.GetFlatRowsAsync(CurrentObject.Sysid);
    }

    async Task LoadConfiguredItems()
    {
        _configuredItems = new List<string>();
        _configuredUnits = new List<string>();
        _configuredDescriptions = new List<string>();
        if (CurrentDoc == null || CurrentObject == null) return;
        var cfg = await DetailSvc.GetConfiguredAsync(CurrentDoc.FormTemplateName, CurrentObject.ObjectType, CurrentObject.ObjectName);
        _configuredItems = cfg.items;
        _configuredUnits = cfg.units;
        _configuredDescriptions = cfg.descriptions;
        _isRequiredSN = cfg.isRequiredSN;
        StateHasChanged();
    }

    async Task NewDetail()
    {
        if (CurrentObject == null || CurrentDoc == null) return;
        _sampleBatchNo = null;
        _bulkDetails = await DetailSvc.BuildDefaultEntriesAsync(CurrentDoc.FormTemplateName, CurrentObject.ObjectName);
        _dlgDetail = true;
    }

    bool CanSaveDetail => CurrentDoc != null && CurrentDoc.Status == "开始" && _targetObjectId != Guid.Empty &&
        (!_isRequiredSN || !string.IsNullOrWhiteSpace(_sampleBatchNo)) &&
        _bulkDetails.All(x => !string.IsNullOrWhiteSpace(x.ItemName)) &&
        _bulkDetails.All(x => !string.IsNullOrWhiteSpace(x.CheckResult));

    async Task SaveDetail()
    {
        if (CurrentObject == null) { _dlgDetail = false; return; }
        await DetailSvc.SaveBatchAsync(CurrentObject, _sampleBatchNo, _bulkDetails);
        _selectedSampleBatchNo = _sampleBatchNo ?? string.Empty;
        _dlgDetail = false;
        await ReloadDetails();
    }

    async Task<IEnumerable<string>> SearchCheckItems(string value, CancellationToken _)
    {
        if (CurrentDoc == null) return Enumerable.Empty<string>();
        return Filter(_configuredItems, value).Result;
    }

    async Task<IEnumerable<string>> SearchUnits(string value, CancellationToken _)
    {
        if (CurrentDoc == null) return Enumerable.Empty<string>();
        return Filter(_configuredUnits, value).Result;
    }

    async Task<IEnumerable<string>> SearchItemDescriptions(string value, CancellationToken _)
    {
        if (CurrentDoc == null) return Enumerable.Empty<string>();
        return Filter(_configuredDescriptions, value).Result;
    }

    static Task<IEnumerable<string>> Filter(IEnumerable<string> src, string value)
    {
        var v = value ?? string.Empty;
        var items = src.Where(x => string.IsNullOrEmpty(v) || x.Contains(v, StringComparison.OrdinalIgnoreCase)).Take(10).ToList();
        return Task.FromResult(items.AsEnumerable());
    }

    void AddDetailRow()
    {
        _bulkDetails.Add(new InspectionEntryRow { CheckResult = "OK" });
    }

    void Close()
    {
        OnClose.InvokeAsync();
    }
}

