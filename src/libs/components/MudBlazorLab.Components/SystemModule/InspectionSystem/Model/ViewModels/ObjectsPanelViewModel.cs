using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using global::InspectionSystem.Models;
using HmiInspection.Models;
using InspectionSystem.Services;

namespace InspectionSystem.Models;

public sealed class ObjectsPanelViewModel
{
    readonly IInspectionConfigService _config;
    readonly IInspectionObjectService _objSvc;
    public ObjectsPanelViewModel(IInspectionConfigService config, IInspectionObjectService objSvc)
    { _config = config; _objSvc = objSvc; }

    public List<InspectionFormObject> Objects { get; private set; } = new();
    public List<InspectionObjectView> ObjectViews { get; private set; } = new();
    public InspectionFormObject? CurrentObject { get; private set; }

    public async Task ReloadAsync(InspectionForm form)
    {
        Objects = await _objSvc.GetObjectsAsync(form.Sysid);
        ObjectViews = await _objSvc.GetObjectViewsAsync(form);
        CurrentObject = Objects.FirstOrDefault();
    }

    public Task<InspectionFormObject> SaveObjectAsync(InspectionForm form, string objectType, string objectName)
        => _objSvc.CreateObjectAsync(form, objectType, objectName);

    public async Task<IEnumerable<string>> SearchObjectTypesAsync(string templateName, string value)
    {
        var src = await _config.GetObjectTypesAsync(templateName);
        var v = value ?? string.Empty;
        return src.Where(x => string.IsNullOrEmpty(v) || x.Contains(v, StringComparison.OrdinalIgnoreCase)).Take(10).ToList();
    }
}

