using InspectionSystem.Data;
using InspectionSystem.Models;

namespace InspectionSystem.Services;

public class InMemoryInspectionConfigService : IInspectionConfigService {
  public InMemoryInspectionConfigService(InspectionDb db) {
    _db = db;
  }
  InspectionDb _db;
  public readonly List<string> _templates = new() { "模板_开班检查_MA5", "模板_开班检查_MA5_FG" };
  public readonly List<string> _lines = new() { "MA5_Assembly", "MA5_FG" };
  public readonly List<string> _creators = new() { "admin@example.com", "user@example.com", "manager@example.com", "editor@example.com" };
  public Task<List<string>> GetTemplateNamesAsync() => Task.FromResult(_templates);
  public Task<string?> GetInspectionLevelAsync(string templateName) => Task.FromResult<string?>("II");
  public Task<List<string>> GetProductionLineNamesAsync() => Task.FromResult(_lines);
  public Task<List<fab_work_order>> GetWorkOrderNamesAsync() {
    return _db.Db.Queryable<fab_work_order>().ToListAsync();
  }
  public Task<List<string>> GetObjectTypesAsync(string templateName) => Task.FromResult(new List<string> { "设备", "工装" });
  public Task<List<string>> GetCheckObjectsAsync(string templateName, string objectType) => Task.FromResult(new List<string> { "主设备", "辅设备" });
  public Task<List<string>> GetCheckItemsAsync(string templateName, string objectType) => Task.FromResult(new List<string> { "高度", "长度", "重量" });
  public Task<List<string>> GetDescriptionsAsync(string templateName, string objectType) => Task.FromResult(new List<string> { "检查尺寸", "检查重量" });
  public Task<List<string>> GetUnitsAsync(string templateName, string objectType) => Task.FromResult(new List<string> { "cm", "mm", "kg" });
  public Task<List<string>> GetCreatorNamesAsync() => Task.FromResult(_creators);
}
