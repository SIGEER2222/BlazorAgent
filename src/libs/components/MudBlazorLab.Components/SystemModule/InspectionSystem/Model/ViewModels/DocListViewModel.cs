using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HmiInspection.Models;
using InspectionSystem.Services;
using global::InspectionSystem.Models;
using MudBlazor;

namespace InspectionSystem.Models;

public sealed class DocListViewModel {
  readonly IInspectionConfigService _config;
  readonly IInspectionFormService _formSvc;
  readonly InspectionSystem.Data.InspectionDb _db;
  public DocListViewModel(IInspectionConfigService config, IInspectionFormService formSvc, InspectionSystem.Data.InspectionDb db) { _config = config; _formSvc = formSvc; _db = db; }

  public InspectionForm? CurrentForm { get; private set; }
  public List<fab_work_order> WorkOrderOptionsAll { get; private set; } = new();
  public DocFormState Form { get; private set; } = new();
  public bool CanSubmitDoc => !string.IsNullOrWhiteSpace(Form.Template) && !string.IsNullOrWhiteSpace(Form.Line) && !string.IsNullOrWhiteSpace(Form.DocNumber) && !string.IsNullOrWhiteSpace(Form.Creator);

  public async Task NewDocAsync(FormTable formType) {
    Form = new DocFormState();
    Form.DocNumber = await _formSvc.GenerateFormNoAsync(formType);
    WorkOrderOptionsAll = (await _config.GetWorkOrderNamesAsync()).ToList();
  }

  public async Task SaveDocAsync(FormTable formType) {
    var doc = new InspectionForm {
      Sysid = Guid.NewGuid(),
      FormTemplateName = Form.Template,
      FormNo = Form.DocNumber,
      FormType = formType.ToString(),
      ProductName = Form.Line,
      WorkCenter = string.Join(",", Form.WorkOrders.Select(x => x.WorkOrder)),
      CreateUser = Form.Creator,
      TxnTime = Form.CreatedAt ?? DateTime.Now,
      Status = "创建"
    };
    await _formSvc.CreateFormAsync(doc);
    CurrentForm = doc;
    if (Form.AutoStartAndCreateObject) {
      await _formSvc.StartFormAsync(doc);
    }
    await _formSvc.CreateObjectsFromTemplateAsync(doc);
  }

  public Task<IEnumerable<string>> SearchTemplatesAsync(FormTable formType, string value)
      => FilterAsync(_config.GetTemplateNamesByFormTypeAsync(formType.ToString()), value);
  public Task<IEnumerable<string>> SearchLinesAsync(string value)
      => FilterAsync(_config.GetProductionLineNamesAsync(), value);
  public Task<IEnumerable<string>> SearchCreatorsAsync(string value)
      => FilterAsync(_config.GetCreatorNamesAsync(), value);

  async Task<IEnumerable<string>> FilterAsync(Task<List<string>> srcTask, string value) {
    var src = await srcTask;
    var v = value ?? string.Empty;
    var items = src.Where(x => string.IsNullOrEmpty(v) || x.Contains(v, StringComparison.OrdinalIgnoreCase)).Take(10).ToList();
    return items.AsEnumerable();
  }

  // 组件侧保持表格数据加载逻辑，VM 专注于表单创建流程
}

public sealed class DocFormState {
  public string Template { get; set; } = string.Empty;
  public string Line { get; set; } = string.Empty;
  public HashSet<fab_work_order> WorkOrders { get; set; } = new();
  public string DocNumber { get; set; } = string.Empty;
  public string Creator { get; set; } = string.Empty;
  public DateTime? CreatedAt { get; set; } = DateTime.Now;
  public bool AutoStartAndCreateObject { get; set; }
}
