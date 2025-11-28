using InspectionSystem.Data;
using HmiInspection.Models;
using InspectionSystem.Models;

namespace InspectionSystem.Services;

public class InspectionFormService : IInspectionFormService {
  readonly InspectionDb _db;
  readonly IInspectionConfigService _config;

  public InspectionFormService(InspectionDb db, IInspectionConfigService config) {
    _db = db;
    _config = config;
  }

  public async Task<string> GenerateFormNoAsync(FormTable formType) {
    var typeCode = GetFormTypeCode(formType);
    var datePart = DateTime.Now.ToString("yyyyMMdd");
    var prefix = $"{typeCode}-{datePart}";
    var countToday = await _db.Db.Queryable<InspectionForm>().Where(x => x.FormNo.StartsWith(prefix)).CountAsync();
    return prefix + "." + (countToday + 1).ToString("D4");
  }

  public Task CreateFormAsync(InspectionForm form) {
    _db.Db.Insertable(form).ExecuteCommand();
    return Task.CompletedTask;
  }

  public Task StartFormAsync(InspectionForm form) {
    form.Status = "开始";
    _db.Db.Updateable(form).ExecuteCommand();
    return Task.CompletedTask;
  }

  public Task EndFormAsync(InspectionForm form) {
    form.Status = "结束";
    _db.Db.Updateable(form).ExecuteCommand();
    return Task.CompletedTask;
  }

  public async Task CreateObjectsFromTemplateAsync(InspectionForm form) {
    var templateObj = await _config.GetTemplateObjectsAsync(form.FormTemplateName);
    var lstObj = templateObj.Select(x => new InspectionFormObject {
      Sysid = Guid.NewGuid(),
      FormSysid = form.Sysid,
      ObjectName = x.ObjectName,
      ObjectType = x.ObjectType,
      CheckResult = InspectionResult.OK.ToString()
    }).ToList();
    _db.Db.Insertable(lstObj).ExecuteCommand();
  }

  static string GetFormTypeCode(FormTable formType) => formType switch {
    FormTable.来料检验单 => "IQC",
    FormTable.过程检验单 => "PQC",
    FormTable.出货检验单 => "OQC",
    FormTable.开班点检单 => "SHIFT",
    FormTable.设备点检单 => "EQP",
    _ => "GEN"
  };
}
