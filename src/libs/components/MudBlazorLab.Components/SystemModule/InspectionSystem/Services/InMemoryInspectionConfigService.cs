using InspectionSystem.Data;
using InspectionSystem.Models;

namespace InspectionSystem.Services;

public class InMemoryInspectionConfigService : IInspectionConfigService {
  public readonly List<string> _templates = new() { "模板_开班检查_MA5", "模板_开班检查_MA5_FG" };
  public readonly List<string> _lines = new() { "MA5_Assembly", "MA5_FG" };
  public readonly List<WorkOrderDto> _workOrders = Enumerable.Range(1, 15).Select(i => new WorkOrderDto { Id = i, OrderNumber = $"工单-{i}", Description = $"工单-{i}" }).ToList();
  public readonly List<string> _creators = new() { "admin@example.com", "user@example.com", "manager@example.com", "editor@example.com" };
  public Task<List<string>> GetTemplateNamesAsync() => Task.FromResult(_templates);
  public Task<string?> GetInspectionLevelAsync(string templateName) => Task.FromResult<string?>("II");
  public Task<List<string>> GetProductionLineNamesAsync() => Task.FromResult(_lines);
  public Task<List<WorkOrderDto>> GetWorkOrderNamesAsync() => Task.FromResult(_workOrders);
  public Task<List<string>> GetObjectTypesAsync(string templateName) => Task.FromResult(new List<string> { "设备", "工装" });
  public Task<List<string>> GetCheckObjectsAsync(string templateName, string objectType) => Task.FromResult(new List<string> { "主设备", "辅设备" });
  public Task<List<string>> GetCheckItemsAsync(string templateName, string objectType) => Task.FromResult(new List<string> { "高度", "长度", "重量" });
  public Task<List<string>> GetDescriptionsAsync(string templateName, string objectType) => Task.FromResult(new List<string> { "检查尺寸", "检查重量" });
  public Task<List<string>> GetUnitsAsync(string templateName, string objectType) => Task.FromResult(new List<string> { "cm", "mm", "kg" });
  public Task<List<string>> GetCreatorNamesAsync() => Task.FromResult(_creators);
}
