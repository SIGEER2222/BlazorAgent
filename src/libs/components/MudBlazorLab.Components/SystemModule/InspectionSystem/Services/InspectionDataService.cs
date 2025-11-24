using InspectionSystem.Data;
using InspectionSystem.Models;

namespace InspectionSystem.Services;

public class InspectionDataService : IInspectionDataService {
    readonly InspectionDb _db;
    public InspectionDataService(InspectionDb db) { _db = db; }
    public Task<(IEnumerable<InspectionDoc> items, int total)> QueryDocsAsync(int page, int pageSize) {
        int total = 0;
        var items = _db.Docs.OrderBy(x => x.CreatedAt, SqlSugar.OrderByType.Desc).ToPageList(page, pageSize, ref total);
        return Task.FromResult(((IEnumerable<InspectionDoc>)items, total));
    }
    public Task CreateDocAsync(InspectionDoc doc) {
        _db.Db.Insertable(doc).ExecuteCommand();
        return Task.CompletedTask;
    }
    public Task<(IEnumerable<InspectionObject> items, int total)> QueryObjectsAsync(string docNumber, int page, int pageSize) {
        int total = 0;
        var items = _db.Objects.Where(x => x.DocNumber == docNumber).OrderBy(x => x.CreatedAt, SqlSugar.OrderByType.Desc).ToPageList(page, pageSize, ref total);
        return Task.FromResult(((IEnumerable<InspectionObject>)items, total));
    }
    public Task AddObjectAsync(InspectionObject obj) {
        var maxId = _db.Db.Queryable<InspectionObject>().Max(x => x.ObjectId);
        obj.ObjectId = maxId + 1;
        _db.Db.Insertable(obj).ExecuteCommand();
        return Task.CompletedTask;
    }
    public Task<(IEnumerable<InspectionDetail> items, int total)> QueryDetailsAsync(int objectId, int page, int pageSize) {
        int total = 0;
        var items = _db.Details.Where(x => x.ObjectId == objectId).OrderBy(x => x.CreatedAt, SqlSugar.OrderByType.Desc).ToPageList(page, pageSize, ref total);
        return Task.FromResult(((IEnumerable<InspectionDetail>)items, total));
    }
    public Task AddDetailAsync(InspectionDetail detail) {
        _db.Db.Insertable(detail).ExecuteCommand();
        var hasNg = _db.Db.Queryable<InspectionDetail>().Where(x => x.ObjectId == detail.ObjectId && x.Result == InspectionResult.NG).Any();
        var obj = _db.Db.Queryable<InspectionObject>().Where(x => x.ObjectId == detail.ObjectId).First();
        if (obj != null) {
            obj.Result = hasNg ? InspectionResult.NG : InspectionResult.OK;
            _db.Db.Updateable(obj).ExecuteCommand();
        }
        return Task.CompletedTask;
    }
    public Task UpdateDocStatusAsync(string docNumber, InspectionStatus status) {
        var doc = _db.Db.Queryable<InspectionDoc>().Where(x => x.DocNumber == docNumber).First();
        if (doc != null) {
            doc.Status = status;
            _db.Db.Updateable(doc).ExecuteCommand();
        }
        return Task.CompletedTask;
    }
}