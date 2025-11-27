using InspectionSystem.Data;
using HmiInspection.Models;

namespace InspectionSystem.Services;

public class InMemoryInspectionConfigService : IInspectionConfigService {
  public InMemoryInspectionConfigService(InspectionDb db) {
    _db = db;
  }
  InspectionDb _db;

  public Task<List<string>> GetTemplateNamesAsync()
    => _db.Db.Queryable<InspectionFormTemplate>().Select(x => x.FormTemplateName).Distinct().ToListAsync();

  public Task<List<string>> GetProductionLineNamesAsync()
    => _db.Db.Queryable<mom_product_revision>().Where(x=>x.RevisionState == "Active").Select(x => x.Name).Distinct().ToListAsync();

  public Task<List<fab_work_order>> GetWorkOrderNamesAsync()
    => _db.Db.Queryable<fab_work_order>().ToListAsync();

  public async Task<List<string>> GetObjectTypesAsync(string templateName) {
    var tpl = await _db.Db.Queryable<InspectionFormTemplate>().Where(x => x.FormTemplateName == templateName).FirstAsync();
    if (tpl == null) return new List<string>();
    return await _db.Db.Queryable<InspectionFormTemplateObject>()
      .Where(x => x.FormTemplateSysid == tpl.Sysid)
      .Select(x => x.ObjectType)
      .Distinct()
      .ToListAsync();
  }

  public async Task<List<string>> GetCheckObjectsAsync(string templateName, string objectType) {
    var tpl = await _db.Db.Queryable<InspectionFormTemplate>().Where(x => x.FormTemplateName == templateName).FirstAsync();
    if (tpl == null) return new List<string>();
    return await _db.Db.Queryable<InspectionFormTemplateObject>()
      .Where(x => x.FormTemplateSysid == tpl.Sysid && x.ObjectType == objectType)
      .Select(x => x.ObjectName)
      .Distinct()
      .ToListAsync();
  }

  public async Task<List<string>> GetCheckItemsAsync(string templateName, string objectType) {
    var tpl = await _db.Db.Queryable<InspectionFormTemplate>().Where(x => x.FormTemplateName == templateName).FirstAsync();
    if (tpl == null) return new List<string>();
    var objIds = await _db.Db.Queryable<InspectionFormTemplateObject>()
      .Where(x => x.FormTemplateSysid == tpl.Sysid && x.ObjectType == objectType)
      .Select(x => x.Sysid)
      .ToListAsync();
    if (objIds.Count == 0) return new List<string>();
    return await _db.Db.Queryable<InspectionFormTemplateObjectItem>()
      .Where(x => objIds.Contains(x.FormTemplateObjectSysid))
      .Select(x => x.ItemName)
      .Distinct()
      .ToListAsync();
  }

  public Task<List<string>> GetCreatorNamesAsync() => _db.Db.Queryable<InspectionForm>().Select(x => x.CreateUser).Distinct().ToListAsync();

  public async Task<List<InspectionFormTemplateObject>> GetTemplateObjectsAsync(string templateName) {
    var tpl = await _db.Db.Queryable<InspectionFormTemplate>().Where(x => x.FormTemplateName == templateName).FirstAsync();
    if (tpl == null) return new List<InspectionFormTemplateObject>();
    return await _db.Db.Queryable<InspectionFormTemplateObject>().Where(x => x.FormTemplateSysid == tpl.Sysid).ToListAsync();
  }

  public async Task<List<InspectionFormTemplateObjectItem>> GetTemplateObjectItemsAsync(string templateName,string ObjectName) {
    var tpl = await _db.Db.Queryable<InspectionFormTemplate>().Where(x => x.FormTemplateName == templateName).FirstAsync();
    var obj = await _db.Db.Queryable<InspectionFormTemplateObject>().Where(x => x.FormTemplateSysid == tpl.Sysid && x.ObjectName == ObjectName).FirstAsync();
    var items = await _db.Db.Queryable<InspectionFormTemplateObjectItem>().Where(x => x.FormTemplateObjectSysid == obj.Sysid).ToListAsync();
    return items;
  }

  public Task<List<InspectionFormTemplateObjectItem>> GetTemplateObjectItemsAsync(Guid templateObjectSysid)
    => _db.Db.Queryable<InspectionFormTemplateObjectItem>().Where(x => x.FormTemplateObjectSysid == templateObjectSysid).ToListAsync();
}
