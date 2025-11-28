using InspectionSystem.Services;
using MudBlazor;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace MudBlazorLab.Components.SystemModule.InspectionSystem.Components.InspectionCommon;

public partial class DocListBase : ComponentBase {
  [Parameter] public FormTable FormType { get; set; }
  [Parameter] public RenderFragment ExtraFields { get; set; }
  [Parameter] public Func<Task> OnNewDocAsync { get; set; }
  [Parameter] public Func<bool> ValidateExtra { get; set; }
  [Parameter] public Action<InspectionForm> MapExtraToForm { get; set; }
  [Parameter] public EventCallback<InspectionForm> OpenObjectsPanel { get; set; }

  [Inject] protected IInspectionConfigService ConfigSvc { get; set; }
  [Inject] protected IInspectionFormService FormSvc { get; set; }
  [Inject] protected IInspectionFacade Facade { get; set; }
  [Inject] protected global::InspectionSystem.Data.InspectionDb Db { get; set; }

  protected MudDataGrid<InspectionForm> InspectionDocGrid;
  protected int _pageSize = 10;
  protected int[] _pageOptions = new[] { 10, 20, 50 };
  protected bool _dlgDoc;
  protected InspectionForm? _currentForm;

  protected sealed class DocFormState {
    public string Template { get; set; } = string.Empty;
    public string DocNumber { get; set; } = string.Empty;
    public string Creator { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public bool AutoStartAndCreateObject { get; set; }
  }
  protected DocFormState Form = new();
  protected bool _canSubmitDoc => !string.IsNullOrWhiteSpace(Form.Template)
      && !string.IsNullOrWhiteSpace(Form.DocNumber) && !string.IsNullOrWhiteSpace(Form.Creator) && (ValidateExtra?.Invoke() ?? true);

  protected override async Task OnAfterRenderAsync(bool firstRender) {
    if (firstRender) {
      await InspectionDocGrid.SetSortAsync(nameof(InspectionForm.TxnTime), SortDirection.Descending, x => x.TxnTime!.Value);
    }
  }

  protected async Task NewDoc() {
    Form = new();
    Form.DocNumber = await FormSvc.GenerateFormNoAsync(FormType);
    _dlgDoc = true;
    if (OnNewDocAsync != null) await OnNewDocAsync();
  }

  protected async Task SaveDoc() {
    var doc = new InspectionForm {
      Sysid = Guid.NewGuid(),
      FormTemplateName = Form.Template,
      FormNo = Form.DocNumber,
      FormType = FormType.ToString(),
      CreateUser = Form.Creator,
      TxnTime = Form.CreatedAt ?? DateTime.Now,
      Status = "创建"
    };
    MapExtraToForm?.Invoke(doc);
    await Facade.CreateFormFlowAsync(doc, Form.AutoStartAndCreateObject);
    _currentForm = doc;
    _dlgDoc = false;
    await InspectionDocGrid.ReloadServerData();
  }

  protected async Task<IEnumerable<string>> SearchTemplates(string value, CancellationToken _) {
    var list = await ConfigSvc.GetTemplateNamesByFormTypeAsync(FormType.ToString());
    return await Filter(list, value);
  }
  protected async Task<IEnumerable<string>> SearchCreators(string value, CancellationToken _) => await Filter(await ConfigSvc.GetCreatorNamesAsync(), value);
  protected static Task<IEnumerable<string>> Filter(IEnumerable<string> src, string value) {
    var v = value ?? string.Empty;
    var items = src.Where(x => string.IsNullOrEmpty(v) || x.Contains(v, StringComparison.OrdinalIgnoreCase)).Take(10).ToList();
    return Task.FromResult(items.AsEnumerable());
  }

  protected Color StatusColorText(string s) => s switch {
    "创建" => Color.Info,
    "开始" => Color.Success,
    "结束" => Color.Secondary,
    _ => Color.Default
  };

  protected async Task StartForm(InspectionForm doc) => await FormSvc.StartFormAsync(doc);
  protected async Task EndForm(InspectionForm doc) => await FormSvc.EndFormAsync(doc);

  protected async Task OnDocRowClick(DataGridRowClickEventArgs<InspectionForm> e) {
    _currentForm = e.Item;
    if (OpenObjectsPanel.HasDelegate) await OpenObjectsPanel.InvokeAsync(e.Item);
  }

  protected async Task<GridData<InspectionForm>> LoadDocs(GridState<InspectionForm> state) {
    var filters = InspectionDocGrid.FilterDefinitions;
    var predicate = FilterHelper<InspectionForm>.BuildExpression(filters);
    Expression<Func<InspectionForm, bool>> typeFilter = x => x.FormType == FormType.ToString();
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
