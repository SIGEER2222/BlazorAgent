using InspectionSystem.Services;
using HmiInspection.Models;
using MudBlazor;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace MudBlazorLab.Components.SystemModule.InspectionSystem.Components.ProcessInspection;

public partial class ProcessDocList : ComponentBase {
  [Inject] IInspectionConfigService ConfigSvc { get; set; }
  [Inject] IInspectionFormService FormSvc { get; set; }
  [Inject] IInspectionFacade Facade { get; set; }
  string FormLine = string.Empty;
  HashSet<fab_work_order> WorkOrders = new();
  List<fab_work_order> _workOrderOptionsAll = new();
  InspectionForm? _currentForm;
  bool _openObjectsPanel;

  async Task OnNewDocAsync() {
    _workOrderOptionsAll = (await ConfigSvc.GetWorkOrderNamesAsync()).ToList();
    WorkOrders = new();
    FormLine = string.Empty;
  }

  bool ValidateExtra() => !string.IsNullOrWhiteSpace(FormLine) && WorkOrders.Any();

  void MapExtraToForm(InspectionForm form) {
    form.ProductName = FormLine;
    form.WorkCenter = string.Join(",", WorkOrders.Select(x => x.WorkOrder));
  }

  async Task<IEnumerable<string>> SearchLines(string value, CancellationToken _) => await Filter(await ConfigSvc.GetProductionLineNamesAsync(), value);
  static Task<IEnumerable<string>> Filter(IEnumerable<string> src, string value) {
    var v = value ?? string.Empty;
    var items = src.Where(x => string.IsNullOrEmpty(v) || x.Contains(v, StringComparison.OrdinalIgnoreCase)).Take(10).ToList();
    return Task.FromResult(items.AsEnumerable());
  }

  IEnumerable<fab_work_order> FilteredWorkOrderOptions => string.IsNullOrWhiteSpace(FormLine)
      ? _workOrderOptionsAll
      : _workOrderOptionsAll.Where(x => x.ProductName == FormLine);

  Task OpenObjectsPanel(InspectionForm doc) {
    _currentForm = doc;
    _openObjectsPanel = true;
    return Task.CompletedTask;
  }
}
