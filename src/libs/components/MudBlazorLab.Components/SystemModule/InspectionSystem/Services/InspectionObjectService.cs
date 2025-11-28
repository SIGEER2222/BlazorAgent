using InspectionSystem.Data;
using HmiInspection.Models;
using InspectionSystem.Models;

namespace InspectionSystem.Services;

public class InspectionObjectService : IInspectionObjectService
{
    readonly InspectionDb _db;

    public InspectionObjectService(InspectionDb db)
    {
        _db = db;
    }

    public Task<List<InspectionFormObject>> GetObjectsAsync(Guid formSysid)
    {
        var list = _db.Db.Queryable<InspectionFormObject>().Where(x => x.FormSysid == formSysid).ToList();
        return Task.FromResult(list);
    }

    public async Task<List<InspectionObjectView>> GetObjectViewsAsync(InspectionForm form)
    {
        var objs = await GetObjectsAsync(form.Sysid);
        var views = objs.Select(x => new InspectionObjectView
        {
            Sysid = x.Sysid,
            ObjectType = x.ObjectType,
            ObjectName = x.ObjectName,
            CheckResult = x.CheckResult,
            FormTemplateName = form.FormTemplateName,
            FormNo = form.FormNo,
            ProductName = form.ProductName,
            CreatedAt = form.TxnTime,
            CreateUser = form.CreateUser,
            WorkCenter = form.WorkCenter
        }).ToList();
        return views;
    }

    public Task<InspectionFormObject> CreateObjectAsync(InspectionForm form, string objectType, string objectName)
    {
        var obj = new InspectionFormObject
        {
            Sysid = Guid.NewGuid(),
            FormSysid = form.Sysid,
            ObjectType = objectType,
            ObjectName = objectName,
            CheckResult = "OK"
        };
        _db.Db.Insertable(obj).ExecuteCommand();
        return Task.FromResult(obj);
    }
}

