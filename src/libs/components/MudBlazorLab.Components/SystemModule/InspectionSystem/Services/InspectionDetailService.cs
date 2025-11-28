using InspectionSystem.Data;
using InspectionSystem.Models;
using HmiInspection.Models;
using System.Linq;

namespace InspectionSystem.Services;

public class InspectionDetailService : IInspectionDetailService {
  readonly InspectionDb _db;
  readonly IInspectionConfigService _config;

  public InspectionDetailService(InspectionDb db, IInspectionConfigService config) {
    _db = db;
    _config = config;
  }

  public Task<List<InspectionFlatRow>> GetFlatRowsAsync(Guid formObjectSysid) {
    var flats = _db.Db.Queryable<InspectionFormObjectSampleFlat>().Where(x => x.FormObjectSysid == formObjectSysid).ToList();
    var map = new Dictionary<string, int>();
    int counter = 1;
    foreach (var f in flats) {
      var key = f.SampleBatchNo ?? string.Empty;
      if (!string.IsNullOrEmpty(key) && !map.ContainsKey(key)) map[key] = counter++;
    }
    var temp = flats.Select(f => new {
      f,
      idx = f.BatchIndex ?? (f.SampleBatchNo != null && map.ContainsKey(f.SampleBatchNo) ? map[f.SampleBatchNo] : 0)
    }).ToList();
    var statusByIdx = temp.GroupBy(t => t.idx).ToDictionary(g => g.Key, g => g.All(t => t.f.CheckResult == "OK") ? "OK" : "NG");
    var rows = temp.Select(t => new InspectionFlatRow {
      SampleBatchNo = t.f.SampleBatchNo ?? string.Empty,
      ItemName = t.f.ItemName,
      ItemDescription = t.f.ItemDescription,
      Unit = t.f.Unit,
      Value = t.f.Value,
      CheckResult = t.f.CheckResult,
      BatchCheckResult = statusByIdx.ContainsKey(t.idx) ? statusByIdx[t.idx] : string.Empty,
      BatchIndex = t.idx
    }).ToList();
    return Task.FromResult(rows);
  }

  public async Task<(List<string> items, List<string> units, List<string> descriptions, bool isRequiredSN)> GetConfiguredAsync(string templateName, string objectType, string objectName) {
    var items = await _config.GetTemplateObjectItemsAsync(templateName, objectName);
    var configuredItems = items.Select(x => x.ItemName).Distinct().ToList();
    var configuredUnits = items.Select(x => x.Unit).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
    var configuredDescriptions = items.Select(x => x.ItemDescription).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
    var tpl = await _db.Db.Queryable<InspectionFormTemplate>().FirstAsync(x => x.FormTemplateName == templateName);
    var tplObj = await _db.Db.Queryable<InspectionFormTemplateObject>().FirstAsync(x => x.FormTemplateSysid == tpl.Sysid && x.ObjectType == objectType && x.ObjectName == objectName);
    var isRequiredSN = tplObj?.IsRequiredSN ?? false;
    return (configuredItems, configuredUnits, configuredDescriptions, isRequiredSN);
  }

  public async Task SaveBatchAsync(InspectionFormObject currentObject, string? sampleBatchNo, List<InspectionEntryRow> rows) {
    var maxIndex = await _db.Db.Queryable<InspectionFormObjectSampleFlat>()
        .Where(x => x.FormObjectSysid == currentObject.Sysid)
        .MaxAsync(it => it.BatchIndex) ?? 0;
    var nextIndex = maxIndex + 1;
    var toInsert = rows.Select(x => new InspectionFormObjectSampleFlat {
      Sysid = Guid.NewGuid(),
      FormObjectSysid = currentObject.Sysid,
      SampleBatchNo = sampleBatchNo,
      ItemName = x.ItemName,
      Unit = x.Unit,
      ItemDescription = x.ItemDescription,
      CheckResult = x.CheckResult,
      Value = decimal.TryParse(x.ValueText, out var v) ? v : null,
      BatchIndex = nextIndex
    }).ToList();
    await _db.Db.Insertable(toInsert).ExecuteCommandAsync();
    var allOK = toInsert.All(x => x.CheckResult == "OK");
    currentObject.CheckResult = allOK ? "OK" : "NG";
    await _db.Db.Updateable(currentObject).ExecuteCommandAsync();
  }

  public async Task<List<InspectionEntryRow>> BuildDefaultEntriesAsync(string templateName, string objectName) {
    var items = await _config.GetTemplateObjectItemsAsync(templateName, objectName);
    var list = items.Select(x => new InspectionEntryRow {
      ItemName = x.ItemName,
      Unit = x.Unit,
      ValueText = string.Empty,
      ItemDescription = x.ItemDescription,
      CheckResult = "OK"
    }).ToList();
    return list;
  }
}
