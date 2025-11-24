using InspectionSystem.Models;

namespace InspectionSystem.Services;

public interface IInspectionDataService {
    Task<(IEnumerable<InspectionDoc> items, int total)> QueryDocsAsync(int page, int pageSize);
    Task CreateDocAsync(InspectionDoc doc);
    Task<(IEnumerable<InspectionObject> items, int total)> QueryObjectsAsync(string docNumber, int page, int pageSize);
    Task AddObjectAsync(InspectionObject obj);
    Task<(IEnumerable<InspectionDetail> items, int total)> QueryDetailsAsync(int objectId, int page, int pageSize);
    Task AddDetailAsync(InspectionDetail detail);
    Task UpdateDocStatusAsync(string docNumber, InspectionStatus status);
}