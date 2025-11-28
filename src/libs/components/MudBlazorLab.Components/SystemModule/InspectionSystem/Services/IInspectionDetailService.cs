using InspectionSystem.Models;
using HmiInspection.Models;

namespace InspectionSystem.Services;

public interface IInspectionDetailService {
  Task<List<InspectionFlatRow>> GetFlatRowsAsync(Guid formObjectSysid);
  Task<(List<string> items, List<string> units, List<string> descriptions, bool isRequiredSN)> GetConfiguredAsync(string templateName, string objectType, string objectName);
  Task SaveBatchAsync(InspectionFormObject currentObject, string? sampleBatchNo, List<InspectionEntryRow> rows);
  Task<List<InspectionEntryRow>> BuildDefaultEntriesAsync(string templateName, string objectName);
}
