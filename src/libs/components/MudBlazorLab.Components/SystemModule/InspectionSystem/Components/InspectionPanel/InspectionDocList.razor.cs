using InspectionSystem.Services;
using HmiInspection.Models;
using MudBlazor;
using Microsoft.AspNetCore.Components;

namespace MudBlazorLab.Components.Components.InspectionPanel;

public partial class InspectionDocList : ComponentBase
{
    [Parameter] public FormTable FormType { get; set; }

    [Inject] IInspectionConfigService ConfigSvc { get; set; }
    [Inject] IInspectionFormService FormSvc { get; set; }
    [Inject] IInspectionFacade Facade { get; set; }
    [Inject] InspectionSystem.Data.InspectionDb Db { get; set; }

    MudDataGrid<InspectionForm> InspectionDocGrid;
    int _pageSize = 10;
    int[] _pageOptions = new[] { 10, 20, 50 };
    bool _dlgDoc;
    InspectionForm? _currentForm;
    List<fab_work_order> _workOrderOptionsAll = new();

    sealed class DocFormState
    {
        public string Template { get; set; } = string.Empty;
        public string Line { get; set; } = string.Empty;
        public HashSet<fab_work_order> WorkOrders { get; set; } = new();
        public string DocNumber { get; set; } = string.Empty;
        public string Creator { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public bool AutoStartAndCreateObject { get; set; }
    }
    DocFormState Form = new();
    bool _canSubmitDoc => !string.IsNullOrWhiteSpace(Form.Template) && !string.IsNullOrWhiteSpace(Form.Line)
        && !string.IsNullOrWhiteSpace(Form.DocNumber) && !string.IsNullOrWhiteSpace(Form.Creator);
    bool _openObjectsPanel;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InspectionDocGrid.SetSortAsync(nameof(InspectionForm.TxnTime), SortDirection.Descending, x => x.TxnTime!.Value);
        }
    }

    async Task NewDoc()
    {
        Form = new();
        Form.DocNumber = await FormSvc.GenerateFormNoAsync(FormType);
        _dlgDoc = true;
        _workOrderOptionsAll = (await ConfigSvc.GetWorkOrderNamesAsync()).ToList();
    }

    async Task SaveDoc()
    {
        var doc = new InspectionForm
        {
            Sysid = Guid.NewGuid(),
            FormTemplateName = Form.Template,
            FormNo = Form.DocNumber,
            FormType = FormType.ToString(),
            ProductName = Form.Line,
            WorkCenter = string.Join(",", Form.WorkOrders.Select(x => x.WorkOrder)),
            CreateUser = Form.Creator,
            TxnTime = Form.CreatedAt ?? DateTime.Now,
            Status = "创建"
        };
        await Facade.CreateFormFlowAsync(doc, Form.AutoStartAndCreateObject);
        _currentForm = doc;
        _dlgDoc = false;
        await InspectionDocGrid.ReloadServerData();
    }

    async Task<IEnumerable<string>> SearchTemplates(string value, CancellationToken _)
    {
        var list = await ConfigSvc.GetTemplateNamesByFormTypeAsync(FormType.ToString());
        return await Filter(list, value);
    }
    async Task<IEnumerable<string>> SearchLines(string value, CancellationToken _) => await Filter(await ConfigSvc.GetProductionLineNamesAsync(), value);
    async Task<IEnumerable<string>> SearchCreators(string value, CancellationToken _) => await Filter(await ConfigSvc.GetCreatorNamesAsync(), value);
    static Task<IEnumerable<string>> Filter(IEnumerable<string> src, string value)
    {
        var v = value ?? string.Empty;
        var items = src.Where(x => string.IsNullOrEmpty(v) || x.Contains(v, StringComparison.OrdinalIgnoreCase)).Take(10).ToList();
        return Task.FromResult(items.AsEnumerable());
    }

    Color StatusColorText(string s) => s switch
    {
        "创建" => Color.Info,
        "开始" => Color.Success,
        "结束" => Color.Secondary,
        _ => Color.Default
    };

    async Task StartForm(InspectionForm doc) => await FormSvc.StartFormAsync(doc);
    async Task EndForm(InspectionForm doc) => await FormSvc.EndFormAsync(doc);

    void OnDocRowClick(DataGridRowClickEventArgs<InspectionForm> e)
    {
        _currentForm = e.Item;
        _openObjectsPanel = true;
    }

    void CloseObjectsPanel()
    {
        _openObjectsPanel = false;
    }

    async Task<GridData<InspectionForm>> LoadDocs(GridState<InspectionForm> state)
    {
        var filters = InspectionDocGrid.FilterDefinitions;
        var predicate = FilterHelper<InspectionForm>.BuildExpression(filters);
        using var repo = new Repository<InspectionForm>(Db.Db);
        var result = await repo.LoadGridDataAsync(
            state,
            selector: x => x,
            orderSelector: sort => FilterHelper<InspectionForm>.BuildOrderSelector(sort),
            filter: predicate
        );
        if (_currentForm == null) _currentForm = result.Items.FirstOrDefault();
        return result;
    }

    IEnumerable<fab_work_order> FilteredWorkOrderOptions => string.IsNullOrWhiteSpace(Form.Line)
        ? _workOrderOptionsAll
        : _workOrderOptionsAll.Where(x => x.ProductName == Form.Line);

    string FormLine
    {
        get => Form.Line;
        set
        {
            Form.Line = value;
            Form.WorkOrders.Clear();
        }
    }
}
