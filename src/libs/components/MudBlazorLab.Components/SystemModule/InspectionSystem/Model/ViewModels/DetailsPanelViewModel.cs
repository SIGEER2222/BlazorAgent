using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using global::InspectionSystem.Models;
using HmiInspection.Models;
using InspectionSystem.Services;

namespace InspectionSystem.Models;

public sealed class DetailsPanelViewModel
{
    readonly IInspectionDetailService _detailSvc;
    readonly IInspectionConfigService _configSvc;
    public DetailsPanelViewModel(IInspectionDetailService d, IInspectionConfigService c){ _detailSvc = d; _configSvc = c; }

    public List<InspectionFlatRow> FlatRows { get; private set; } = new();
    public List<string> ConfiguredItems { get; private set; } = new();
    public List<string> ConfiguredUnits { get; private set; } = new();
    public List<string> ConfiguredDescriptions { get; private set; } = new();
    public bool IsRequiredSN { get; private set; }
    public List<InspectionEntryRow> BulkDetails { get; private set; } = new();

    public async Task ReloadDetailsAsync(Guid objectId)
        => FlatRows = await _detailSvc.GetFlatRowsAsync(objectId);

    public async Task LoadConfiguredAsync(string templateName, string objectType, string objectName)
    {
        var cfg = await _detailSvc.GetConfiguredAsync(templateName, objectType, objectName);
        ConfiguredItems = cfg.items;
        ConfiguredUnits = cfg.units;
        ConfiguredDescriptions = cfg.descriptions;
        IsRequiredSN = cfg.isRequiredSN;
    }

    public Task<List<InspectionEntryRow>> BuildDefaultEntriesAsync(string templateName, string objectName)
        => _detailSvc.BuildDefaultEntriesAsync(templateName, objectName);

    public Task SaveBatchAsync(InspectionFormObject currentObject, string? sampleBatchNo)
        => _detailSvc.SaveBatchAsync(currentObject, sampleBatchNo, BulkDetails);
}

