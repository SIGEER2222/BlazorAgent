using InspectionSystem.Data;
using InspectionSystem.Models;

namespace InspectionSystem.Services;

public interface IInspectionConfigService {
  Task<List<string>> GetTemplateNamesAsync();
  Task<string?> GetInspectionLevelAsync(string templateName);
  Task<List<string>> GetProductionLineNamesAsync();
  Task<List<fab_work_order>> GetWorkOrderNamesAsync();
  Task<List<string>> GetObjectTypesAsync(string templateName);
  Task<List<string>> GetCheckObjectsAsync(string templateName, string objectType);
  Task<List<string>> GetCheckItemsAsync(string templateName, string objectType);
  Task<List<string>> GetDescriptionsAsync(string templateName, string objectType);
  Task<List<string>> GetUnitsAsync(string templateName, string objectType);
  Task<List<string>> GetCreatorNamesAsync();
}