using Bogus;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazorLab.Components.Models;
using MudBlazorLab.Components.Services;
using MudBlazorLab.Components.Components;

namespace MudBlazorLab.Components.Components.InspectionPanel;

public partial class IncomingInspectionPanel : ComponentBase {
    [Parameter] public EventCallback<ObjectRow> OnSamplesUpdated { get; set; }
    [Inject] IDialogService Dialog { get; set; } = default!;

    List<InspectionTemplate> Templates = new();
    InspectionTemplate? SelectedTemplate;
    string ErpInput = string.Empty;

    List<DocHeader> DocHeaders = new();
    DocHeader? SelectedDoc;

    List<ObjectRow> ObjectRows = new();
    ObjectRow? SelectedObject;

    List<SnRow> SnRows = new();
    readonly Dictionary<string, List<ObjectRow>> _objectsByDoc = new();
    readonly Dictionary<string, List<SnRow>> _snByObject = new();

    bool CanCreateSample => SelectedDoc != null && SelectedObject != null && (SelectedDoc.StatusText == "创建" || SelectedDoc.StatusText == "开始");

    void SetTemplate(InspectionTemplate? t) { SelectedTemplate = t; }

    void AddDoc() {
        var faker = new Faker("zh_CN");
        var doc = new DocHeader {
            Template = SelectedTemplate?.Name ?? "模板_来料检验单",
            DocNo = $"M{DateTime.Now:yyyyMMdd}-{DocHeaders.Count + 1:0000}",
            ErpNo = string.IsNullOrWhiteSpace(ErpInput) ? $"RCV{DateTime.Now:yyMMdd}{DocHeaders.Count + 6:0000}" : ErpInput,
            StatusText = "创建",
            CreatedAt = DateTime.Now,
            Creator = faker.Name.FullName()
        };
        DocHeaders.Add(doc);
        SelectedDoc = doc;
        ObjectRows = GenerateObjects(doc);
        SnRows = GenerateSn(ObjectRows.FirstOrDefault());
    }

    Task OnDocRowClick(DataGridRowClickEventArgs<DocHeader> args) {
        SelectedDoc = args.Item;
        ObjectRows = GenerateObjects(args.Item);
        SnRows = GenerateSn(ObjectRows.FirstOrDefault());
        return Task.CompletedTask;
    }

    Task OnObjectRowClick(DataGridRowClickEventArgs<ObjectRow> args) {
        SelectedObject = args.Item;
        SnRows = GenerateSn(args.Item);
        return Task.CompletedTask;
    }

    async Task AddObjectRowFor(DocHeader doc) {
        SelectedDoc = doc;
        ObjectRows = GenerateObjects(doc);
        SnRows = GenerateSn(ObjectRows.FirstOrDefault());
        if (doc.StatusText != "开始" || SelectedTemplate == null) return;
        var faker = new Faker("zh_CN");
        var qty = faker.Random.Int(50, 200);
        var sampleSize = AqlService.ComputeSampleSize(qty, SelectedTemplate.InspectionLevel);
        var rate = Math.Round(sampleSize * 100.0 / Math.Max(qty, 1), 1);
        var defaults = new ObjectRow {
            DocNo = doc.DocNo,
            ObjectType = "来料物料",
            ObjectName = faker.Commerce.ProductName(),
            Batch = $"B{DateTime.Now:yyMMdd}-{ObjectRows.Count + 1:000}",
            Total = qty,
            SampleRateText = rate + "%",
            Result = "OK",
            CreatedAt = DateTime.Now,
            Creator = doc.Creator
        };
        IDialogReference? dlg = null;
        var prm = new DialogParameters
        {
            [nameof(AddObjectDialog.Doc)] = doc,
            [nameof(AddObjectDialog.Template)] = SelectedTemplate,
            [nameof(AddObjectDialog.OnCancel)] = EventCallback.Factory.Create(this, () => dlg?.Close()),
            [nameof(AddObjectDialog.OnSubmit)] = EventCallback.Factory.Create<ObjectRow>(this, (ObjectRow m) => {
                var q = Math.Max(m.Total, 1);
                var s = AqlService.ComputeSampleSize(q, SelectedTemplate.InspectionLevel);
                m.SampleRateText = Math.Round(s * 100.0 / q, 1) + "%";
                m.DocNo = doc.DocNo;
                m.CreatedAt = DateTime.Now;
                m.Creator = doc.Creator;
                ObjectRows.Add(m);
                if (!_objectsByDoc.TryGetValue(doc.DocNo, out var list))
                    _objectsByDoc[doc.DocNo] = new List<ObjectRow> { m };
                else
                    list.Add(m);
                SelectedObject = m;
                SnRows = GenerateSn(m);
                StateHasChanged();
                dlg?.Close();
            })
        };
        dlg = await Dialog.ShowAsync<AddObjectDialog>("新增对象明细", prm);
    }

    void StartDocFor(DocHeader doc) {
        SelectedDoc = doc;
        ObjectRows = GenerateObjects(doc);
        SnRows = GenerateSn(ObjectRows.FirstOrDefault());
        if (doc.StatusText == "创建")
            doc.StatusText = "开始";
        StateHasChanged();
    }

    void EndDocFor(DocHeader doc) {
        SelectedDoc = doc;
        if (doc.StatusText == "开始")
            doc.StatusText = "结束";
        StateHasChanged();
    }

    List<ObjectRow> GenerateObjects(DocHeader doc) {
        if (_objectsByDoc.TryGetValue(doc.DocNo, out var cached)) return cached;
        var faker = new Faker("zh_CN");
        var list = new List<ObjectRow>();
        var items = new[]
        {
            new { Type = "生产批次", Name = "MA5_上盖" },
            new { Type = "生产批次", Name = "MA5_下壳" },
            new { Type = "来料物料", Name = "LX_壳体" },
        };
        var totals = new[] { faker.Random.Int(50, 200), faker.Random.Int(80, 300), faker.Random.Int(60, 150) };
        for (int i = 0; i < items.Length; i++) {
            var qty = totals[i];
            var sampleSize = AqlService.ComputeSampleSize(qty, SelectedTemplate?.InspectionLevel ?? "II");
            var rate = Math.Round(sampleSize * 100.0 / Math.Max(qty, 1), 1);
            list.Add(new ObjectRow {
                DocNo = doc.DocNo,
                ObjectType = items[i].Type,
                ObjectName = items[i].Name,
                Batch = $"B{DateTime.Now:yyMMdd}-{i + 1:000}",
                Total = qty,
                SampleRateText = rate + "%",
                Result = "OK",
                CreatedAt = DateTime.Now,
                Creator = doc.Creator
            });
        }
        _objectsByDoc[doc.DocNo] = list;
        return list;
    }

    List<SnRow> GenerateSn(ObjectRow? obj) {
        if (obj == null || SelectedTemplate == null) return new List<SnRow>();
        if (_snByObject.TryGetValue(obj.Batch, out var cached)) return cached;
        var rows = new List<SnRow>();
        var faker = new Faker("zh_CN");
        var sn = faker.Random.Replace("SN-####");
        foreach (var item in SelectedTemplate.Items) {
            var value = SelectedDoc?.StatusText == "创建" ? Math.Round(faker.Random.Double(0.001, 10.0), 3) : double.NaN;
            var r = EvaluateItem(item, value) ? "OK" : "NG";
            rows.Add(new SnRow {
                Sn = sn,
                Item = item.Name,
                Result = r,
                Value = value,
                Unit = item.Kind == "weight" ? "g" : "cm"
            });
        }
        UpdateObjectResult(obj, rows);
        _snByObject[obj.Batch] = rows;
        return rows;
    }

    void AddSample(string? sn = null) {
        if (SelectedObject == null || SelectedTemplate == null) return;
        var faker = new Faker("zh_CN");
        sn ??= faker.Random.Replace("SN-####");
        foreach (var item in SelectedTemplate.Items) {
            var value = SelectedDoc?.StatusText == "创建" ? Math.Round(faker.Random.Double(0.001, 10.0), 3) : double.NaN;
            var r = EvaluateItem(item, value) ? "OK" : "NG";
            SnRows.Add(new SnRow {
                Sn = sn,
                Item = item.Name,
                Result = r,
                Value = value,
                Unit = item.Kind == "weight" ? "g" : "cm"
            });
        }
        UpdateObjectResult(SelectedObject, SnRows);
        OnSamplesUpdated.InvokeAsync(SelectedObject);
        _snByObject[SelectedObject.Batch] = SnRows.ToList();
        StateHasChanged();
    }

    async Task AddSampleFor(ObjectRow obj) {
        SelectedObject = obj;
        SnRows = GenerateSn(obj);
        if ((SelectedDoc?.StatusText ?? string.Empty) != "开始") return;
        var faker = new Faker("zh_CN");
        IDialogReference? dlg = null;
        var prm = new DialogParameters
        {
            [nameof(AddSampleDialog.Template)] = SelectedTemplate,
            [nameof(AddSampleDialog.Target)] = obj,
            [nameof(AddSampleDialog.OnCancel)] = EventCallback.Factory.Create(this, () => dlg?.Close()),
            [nameof(AddSampleDialog.OnSubmit)] = EventCallback.Factory.Create<List<SnRow>>(this, (List<SnRow> rows) => {
                foreach (var r in rows)
                {
                    SnRows.Add(r);
                }
                UpdateObjectResult(SelectedObject!, SnRows);
                OnSamplesUpdated.InvokeAsync(SelectedObject);
                _snByObject[SelectedObject!.Batch] = SnRows.ToList();
                StateHasChanged();
                dlg?.Close();
            })
        };
        var opts = new DialogOptions { MaxWidth = MaxWidth.ExtraLarge, FullWidth = true, CloseOnEscapeKey = true };
        dlg = await Dialog.ShowAsync<AddSampleDialog>("新增样本", prm, opts);
    }

    void OnSampleValueChanged(SnRow row, double value) {
        row.Value = value;
        var item = SelectedTemplate?.Items.FirstOrDefault(x => x.Name == row.Item);
        row.Result = item != null && EvaluateItem(item, value) ? "OK" : "NG";
        if (SelectedObject != null) {
            UpdateObjectResult(SelectedObject, SnRows);
            OnSamplesUpdated.InvokeAsync(SelectedObject);
        }
        StateHasChanged();
    }

    void UpdateObjectResult(ObjectRow obj, List<SnRow> rows) {
        if (SelectedTemplate == null) {
            obj.Result = rows.Any(x => x.Result == "NG") ? "NG" : "OK";
            return;
        }
        var requiredNames = SelectedTemplate.Items.Where(x => x.Required).Select(x => x.Name).ToHashSet();
        var ng = rows.Any(r => requiredNames.Contains(r.Item) && r.Result == "NG");
        obj.Result = ng ? "NG" : "OK";
    }

    bool EvaluateItem(InspectionItem item, double value) {
        if (item.Required && double.IsNaN(value)) return false;
        var t = item.Threshold ?? string.Empty;
        if (string.IsNullOrWhiteSpace(t)) return true;
        if (t.Contains("-")) {
            var parts = t.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && double.TryParse(parts[0], out var min) && double.TryParse(parts[1], out var max)) return value >= min && value <= max;
        }
        if (t.StartsWith(">=")) {
            if (double.TryParse(t.Substring(2), out var m)) return value >= m;
        }
        if (t.StartsWith("<=")) {
            if (double.TryParse(t.Substring(2), out var m)) return value <= m;
        }
        return true;
    }

    protected override void OnInitialized() {
        var faker = new Faker("zh_CN");
        Templates = new List<InspectionTemplate>
        {
            new InspectionTemplate
            {
                Name = "模板_来料检验单_LX",
                Type = InspectionType.IQC,
                InspectionLevel = "II",
                Items = new List<InspectionItem>
                {
                    new InspectionItem { Name = "高度", Kind = "dimension", Required = true, Threshold = "0.1-1.0" },
                    new InspectionItem { Name = "长度", Kind = "dimension", Required = true, Threshold = "0.1-2.0" },
                    new InspectionItem { Name = "重量", Kind = "weight", Required = false, Threshold = ">=0.01" }
                }
            },
            new InspectionTemplate
            {
                Name = "模板_来料检验单_MA5",
                Type = InspectionType.IQC,
                InspectionLevel = "II",
                Items = new List<InspectionItem>
                {
                    new InspectionItem { Name = "厚度", Kind = "dimension", Required = true, Threshold = "0.05-0.5" },
                    new InspectionItem { Name = "硬度", Kind = "dimension", Required = true, Threshold = "0.1-1.5" },
                    new InspectionItem { Name = "重量", Kind = "weight", Required = false, Threshold = "<=5" }
                }
            }
        };
        SelectedTemplate = Templates.First();
        ErpInput = faker.Random.Replace("RCV##########");
        AddDoc();
    }

    class SnInput { public string Sn { get; set; } = string.Empty; }

    async Task OpenAddDocDialog()
    {
        IDialogReference? dlg = null;
        var prm = new DialogParameters
        {
            [nameof(AddDocDialog.Templates)] = Templates,
            [nameof(AddDocDialog.OnCancel)] = EventCallback.Factory.Create(this, () => dlg?.Close()),
            [nameof(AddDocDialog.OnSubmit)] = EventCallback.Factory.Create<DocHeader>(this, (DocHeader doc) =>
            {
                DocHeaders.Insert(0, doc);
                SelectedDoc = doc;
                ObjectRows = GenerateObjects(doc);
                SnRows = GenerateSn(ObjectRows.FirstOrDefault());
                StateHasChanged();
                dlg?.Close();
            })
        };
        dlg = await Dialog.ShowAsync<AddDocDialog>("新增表单", prm);
    }

}
