using InspectionSystem.Models;

namespace InspectionSystem.Services;

public class InMemoryInspectionDataService : IInspectionDataService {
    readonly List<InspectionDoc> _docs = new();
    readonly List<InspectionObject> _objects = new();
    readonly List<InspectionDetail> _details = new();
    public Task<(IEnumerable<InspectionDoc> items, int total)> QueryDocsAsync(int page, int pageSize) {
        var total = _docs.Count;
        var items = _docs.OrderByDescending(x => x.CreatedAt).Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize).ToList();
        return Task.FromResult(((IEnumerable<InspectionDoc>)items, total));
    }
    public Task CreateDocAsync(InspectionDoc doc) {
        _docs.Add(doc);
        return Task.CompletedTask;
    }
    public Task<(IEnumerable<InspectionObject> items, int total)> QueryObjectsAsync(string docNumber, int page, int pageSize) {
        var list = _objects.Where(x => x.DocNumber == docNumber).OrderByDescending(x => x.CreatedAt).ToList();
        var total = list.Count;
        var items = list.Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize).ToList();
        return Task.FromResult(((IEnumerable<InspectionObject>)items, total));
    }
    public Task AddObjectAsync(InspectionObject obj) {
        obj.ObjectId = (_objects.Count == 0 ? 0 : _objects.Max(x => x.ObjectId)) + 1;
        _objects.Add(obj);
        return Task.CompletedTask;
    }
    public Task<(IEnumerable<InspectionDetail> items, int total)> QueryDetailsAsync(int objectId, int page, int pageSize) {
        var list = _details.Where(x => x.ObjectId == objectId).OrderByDescending(x => x.CreatedAt).ToList();
        var total = list.Count;
        var items = list.Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize).ToList();
        return Task.FromResult(((IEnumerable<InspectionDetail>)items, total));
    }
    public Task AddDetailAsync(InspectionDetail detail) {
        _details.Add(detail);
        var hasNg = _details.Any(x => x.ObjectId == detail.ObjectId && x.Result == InspectionResult.NG);
        var obj = _objects.FirstOrDefault(x => x.ObjectId == detail.ObjectId);
        if (obj != null)
            obj.Result = hasNg ? InspectionResult.NG : InspectionResult.OK;
        return Task.CompletedTask;
    }
    public Task UpdateDocStatusAsync(string docNumber, InspectionStatus status) {
        var doc = _docs.FirstOrDefault(x => x.DocNumber == docNumber);
        if (doc != null) doc.Status = status;
        return Task.CompletedTask;
    }
}