using InspectionSystem.Services;
using MudBlazor;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace MudBlazorLab.Components.SystemModule.InspectionSystem.Components.IncomingInspection;

public partial class IncomingDocList : ComponentBase {
  [Inject] IInspectionConfigService ConfigSvc { get; set; }
  [Inject] IInspectionFormService FormSvc { get; set; }
  [Inject] IInspectionFacade Facade { get; set; }
  [Inject] global::InspectionSystem.Data.InspectionDb Db { get; set; }

  MudDataGrid<InspectionForm> InspectionDocGrid;
  int _pageSize = 10;
  int[] _pageOptions = new[] { 10, 20, 50 };
  bool _dlgDoc;
  InspectionForm? _currentForm;
  List<global::InspectionSystem.Models.ErpTicket> _erpTickets = new();

  sealed class DocFormState {
    public string Template { get; set; } = string.Empty;
    public HashSet<global::InspectionSystem.Models.ErpTicket> Tickets { get; set; } = new();
    public string DocNumber { get; set; } = string.Empty;
    public string Creator { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public bool AutoStartAndCreateObject { get; set; }
  }
  DocFormState Form = new();
  bool _canSubmitDoc => !string.IsNullOrWhiteSpace(Form.Template)
      && !string.IsNullOrWhiteSpace(Form.DocNumber) && !string.IsNullOrWhiteSpace(Form.Creator) && Form.Tickets.Any();
  bool _openObjectsPanel;

  protected override async Task OnAfterRenderAsync(bool firstRender) {
    if (firstRender) {
      await InspectionDocGrid.SetSortAsync(nameof(InspectionForm.TxnTime), SortDirection.Descending, x => x.TxnTime!.Value);
    }
  }

  async Task NewDoc() {
    Form = new();
    Form.DocNumber = await FormSvc.GenerateFormNoAsync(FormTable.来料检验单);
    _dlgDoc = true;
    _erpTickets = (await ConfigSvc.GetErpTicketNosAsync()).ToList();
  }

  async Task SaveDoc() {
    var doc = new InspectionForm {
      Sysid = Guid.NewGuid(),
      FormTemplateName = Form.Template,
      FormNo = Form.DocNumber,
      FormType = FormTable.来料检验单.ToString(),
      ProductName = string.Empty,
      TicketNo = Form.Tickets.FirstOrDefault()?.TicketNo,
      CreateUser = Form.Creator,
      TxnTime = Form.CreatedAt ?? DateTime.Now,
      Status = "创建"
    };
    await Facade.CreateFormFlowAsync(doc, Form.AutoStartAndCreateObject);
    _currentForm = doc;
    _dlgDoc = false;
    await InspectionDocGrid.ReloadServerData();
  }

  async Task<IEnumerable<string>> SearchTemplates(string value, CancellationToken _) {
    var list = await ConfigSvc.GetTemplateNamesByFormTypeAsync(FormTable.开班点检单.ToString());
    return await Filter(list, value);
  }
  async Task<IEnumerable<string>> SearchCreators(string value, CancellationToken _) => await Filter(await ConfigSvc.GetCreatorNamesAsync(), value);
  static Task<IEnumerable<string>> Filter(IEnumerable<string> src, string value) {
    var v = value ?? string.Empty;
    var items = src.Where(x => string.IsNullOrEmpty(v) || x.Contains(v, StringComparison.OrdinalIgnoreCase)).Take(10).ToList();
    return Task.FromResult(items.AsEnumerable());
  }

  Color StatusColorText(string s) => s switch {
    "创建" => Color.Info,
    "开始" => Color.Success,
    "结束" => Color.Secondary,
    _ => Color.Default
  };

  async Task StartForm(InspectionForm doc) => await FormSvc.StartFormAsync(doc);
  async Task EndForm(InspectionForm doc) => await FormSvc.EndFormAsync(doc);

  void OnDocRowClick(DataGridRowClickEventArgs<InspectionForm> e) {
    _currentForm = e.Item;
    _openObjectsPanel = true;
  }

  void CloseObjectsPanel() {
    _openObjectsPanel = false;
  }

  async Task<GridData<InspectionForm>> LoadDocs(GridState<InspectionForm> state) {
    var filters = InspectionDocGrid.FilterDefinitions;
    var predicate = FilterHelper<InspectionForm>.BuildExpression(filters);
    Expression<Func<InspectionForm, bool>> typeFilter = x => x.FormType == FormTable.来料检验单.ToString();
    var finalFilter = FilterHelper<InspectionForm>.And(typeFilter, predicate);
    using var repo = new Repository<InspectionForm>(Db.Db);
    var result = await repo.LoadGridDataAsync(
        state,
        selector: x => x,
        orderSelector: sort => FilterHelper<InspectionForm>.BuildOrderSelector(sort),
        filter: finalFilter
    );
    if (_currentForm == null) _currentForm = result.Items.FirstOrDefault();
    return result;
  }


}
