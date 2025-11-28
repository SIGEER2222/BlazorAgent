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
            CarrierName = x.CarrierName,
            SampleBatchNo = x.SampleBatchNo,
            TotalQuantity = x.TotalQuantity,
            ActualSamplingRatio = x.ActualSamplingRatio,
            SampleQuantity = x.SampleQuantity,
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

    public Task<InspectionFormObject> CreateObjectAsync(InspectionForm form, string objectType, string objectName, string? carrierName, string? batchNo)
    {
        var obj = new InspectionFormObject
        {
            Sysid = Guid.NewGuid(),
            FormSysid = form.Sysid,
            ObjectType = objectType,
            ObjectName = objectName,
            CarrierName = carrierName,
            SampleBatchNo = batchNo,
            CheckResult = "OK"
        };
        _db.Db.Insertable(obj).ExecuteCommand();
        return Task.FromResult(obj);
    }

    public Task<InspectionFormObject> CreateObjectAsync(InspectionForm form, string objectType, string objectName, string? batchNo, long? totalQty, decimal? samplingRatio)
    {
        decimal? templateRatio = null;
        var tpl = _db.Db.Queryable<HmiInspection.Models.InspectionFormTemplate>().Where(x => x.FormTemplateName == form.FormTemplateName).First();
        if (tpl != null)
        {
            var tObj = _db.Db.Queryable<HmiInspection.Models.InspectionFormTemplateObject>().Where(x => x.FormTemplateSysid == tpl.Sysid && x.ObjectName == objectName).First();
            templateRatio = tObj?.SamplingRatio;
        }
        long? sampleQty = null;
        if (totalQty.HasValue && samplingRatio.HasValue)
        {
            sampleQty = (long)Math.Ceiling(totalQty.Value * samplingRatio.Value);
        }
        var obj = new InspectionFormObject
        {
            Sysid = Guid.NewGuid(),
            FormSysid = form.Sysid,
            ObjectType = objectType,
            ObjectName = objectName,
            SampleBatchNo = batchNo,
            TotalQuantity = totalQty,
            ActualSamplingRatio = samplingRatio,
            SampleQuantity = sampleQty,
            TemplateSamplingRatio = templateRatio,
            CheckResult = "OK"
        };
        _db.Db.Insertable(obj).ExecuteCommand();
        return Task.FromResult(obj);
    }
}
