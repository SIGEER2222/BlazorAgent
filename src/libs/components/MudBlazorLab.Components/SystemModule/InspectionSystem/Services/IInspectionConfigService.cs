using HmiInspection.Models;

namespace InspectionSystem.Services;

public interface IInspectionConfigService {
  Task<List<string>> GetTemplateNamesAsync();
  Task<List<string>> GetTemplateNamesByFormTypeAsync(string formType);
  Task<List<string>> GetProductionLineNamesAsync();
  Task<List<fab_work_order>> GetWorkOrderNamesAsync();
  Task<List<string>> GetObjectTypesAsync(string templateName);
  Task<List<string>> GetCheckObjectsAsync(string templateName, string objectType);
  Task<List<string>> GetCheckItemsAsync(string templateName, string objectType);
  Task<List<string>> GetCreatorNamesAsync();
  Task<List<InspectionFormTemplateObject>> GetTemplateObjectsAsync(string templateName);
  Task<List<InspectionFormTemplateObjectItem>> GetTemplateObjectItemsAsync(Guid templateObjectSysid);
  Task<List<InspectionFormTemplateObjectItem>> GetTemplateObjectItemsAsync(string templateName, string ObjectName);
}
