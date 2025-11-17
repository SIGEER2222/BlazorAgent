using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using MudBlazorLab.Components.Models;

namespace MudBlazorLab.Components.Components.InspectionPanel;

public partial class LogTable : ComponentBase {
    [Parameter] public IEnumerable<LogEntry>? Source { get; set; }
    [Parameter] public string Height { get; set; } = "70vh";
    [Parameter] public int PageSize { get; set; } = 20;
    [Parameter] public int[] PageSizeOptions { get; set; } = new[] { 10, 20, 50, 100, 500 };
    [Parameter] public RenderFragment? Toolbar { get; set; }

    bool _typeFilterOpen { get; set; }
    bool _lineFilterOpen { get; set; }
    bool _objectFilterOpen { get; set; }
    HashSet<LogMessageType> SelectedTypes { get; set; } = new();
    HashSet<string> SelectedLines { get; set; } = new();
    HashSet<string> SelectedObjects { get; set; } = new();
    HashSet<string> SelectedObjectTypes { get; set; } = new();
    bool _timeFilterOpen { get; set; }
    DateTime? SelectedDate { get; set; }
    bool _contentFilterOpen { get; set; }
    string SelectedContentText { get; set; } = string.Empty;


    IEnumerable<LogMessageType> TypeOptions => Enum.GetValues(typeof(LogMessageType)).Cast<LogMessageType>();
    IEnumerable<string> LineOptions => (Source ?? Enumerable.Empty<LogEntry>()).Select(x => x.Line).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x);
    IEnumerable<string> ObjectOptions => (Source ?? Enumerable.Empty<LogEntry>()).Select(x => x.Object).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x);
    IEnumerable<string> ObjectTypeOptions => (Source ?? Enumerable.Empty<LogEntry>()).Select(x => x.ObjectType).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x);

    IEnumerable<LogEntry> ViewItems => (Source ?? Enumerable.Empty<LogEntry>()).OrderByDescending(x => x.Time);

    MudBlazor.FilterDefinition<LogEntry> _typeFilterDef = new();
    MudBlazor.FilterDefinition<LogEntry> _lineFilterDef = new();
    MudBlazor.FilterDefinition<LogEntry> _objectFilterDef = new();
    MudBlazor.FilterDefinition<LogEntry> _objectTypeFilterDef = new();
    MudBlazor.FilterDefinition<LogEntry> _timeFilterDef = new();
    MudBlazor.FilterDefinition<LogEntry> _contentFilterDef = new();

    Task OnSelectedDateChangedAsync(DateTime? value, MudBlazor.FilterContext<LogEntry> context) {
        SelectedDate = value;
        _timeFilterDef.FilterFunction = x => !SelectedDate.HasValue || x.Time.Date == SelectedDate.Value.Date;
        return context.Actions.ApplyFilterAsync(_timeFilterDef);
    }

    Task OnSelectedContentTextChangedAsync(string value, MudBlazor.FilterContext<LogEntry> context) {
        SelectedContentText = value ?? string.Empty;
        var token = SelectedContentText.Trim().ToLowerInvariant();
        _contentFilterDef.FilterFunction = x => string.IsNullOrWhiteSpace(token) || (!string.IsNullOrEmpty(x.Content) && x.Content.ToLowerInvariant().Contains(token));
        return context.Actions.ApplyFilterAsync(_contentFilterDef);
    }


    MudBlazor.Color TypeColor(LogMessageType t)
        => t switch {
            LogMessageType.Info => MudBlazor.Color.Info,
            LogMessageType.Error => MudBlazor.Color.Error,
            LogMessageType.Alarm => MudBlazor.Color.Warning,
            _ => MudBlazor.Color.Default
        };

    string TypeIcon(LogMessageType t)
        => t switch {
            LogMessageType.Info => MudBlazor.Icons.Material.Filled.Info,
            LogMessageType.Error => MudBlazor.Icons.Material.Filled.Error,
            LogMessageType.Alarm => MudBlazor.Icons.Material.Filled.Warning,
            _ => MudBlazor.Icons.Material.Filled.Info
        };

    Task OnSelectedTypesChangedAsync(IEnumerable<LogMessageType> values, MudBlazor.FilterContext<LogEntry> context) {
        SelectedTypes = new HashSet<LogMessageType>(values ?? Array.Empty<LogMessageType>());
        _typeFilterDef.FilterFunction = x => SelectedTypes.Count == 0 || SelectedTypes.Contains(x.Type);
        return context.Actions.ApplyFilterAsync(_typeFilterDef);
    }

    Task OnSelectedLinesChangedAsync(IEnumerable<string> values, MudBlazor.FilterContext<LogEntry> context) {
        SelectedLines = new HashSet<string>(values ?? Array.Empty<string>());
        _lineFilterDef.FilterFunction = x => SelectedLines.Count == 0 || (!string.IsNullOrEmpty(x.Line) && SelectedLines.Contains(x.Line));
        return context.Actions.ApplyFilterAsync(_lineFilterDef);
    }

    Task OnSelectedObjectsChangedAsync(IEnumerable<string> values, MudBlazor.FilterContext<LogEntry> context) {
        SelectedObjects = new HashSet<string>(values ?? Array.Empty<string>());
        _objectFilterDef.FilterFunction = x => SelectedObjects.Count == 0 || (!string.IsNullOrEmpty(x.Object) && SelectedObjects.Contains(x.Object));
        return context.Actions.ApplyFilterAsync(_objectFilterDef);
    }

    Task ApplyTypeFilter(HashSet<LogMessageType> values, MudBlazor.FilterContext<LogEntry> context) {
        SelectedTypes = values;
        _typeFilterDef.FilterFunction = x => SelectedTypes.Count == 0 || SelectedTypes.Contains(x.Type);
        return context.Actions.ApplyFilterAsync(_typeFilterDef);
    }

    Task ApplyLineFilter(HashSet<string> values, MudBlazor.FilterContext<LogEntry> context) {
        SelectedLines = values;
        _lineFilterDef.FilterFunction = x => SelectedLines.Count == 0 || (!string.IsNullOrEmpty(x.Line) && SelectedLines.Contains(x.Line));
        return context.Actions.ApplyFilterAsync(_lineFilterDef);
    }

    Task ApplyObjectFilter(HashSet<string> values, MudBlazor.FilterContext<LogEntry> context) {
        SelectedObjects = values;
        _objectFilterDef.FilterFunction = x => SelectedObjects.Count == 0 || (!string.IsNullOrEmpty(x.Object) && SelectedObjects.Contains(x.Object));
        return context.Actions.ApplyFilterAsync(_objectFilterDef);
    }

    Task ApplyObjectTypeFilter(HashSet<string> values, MudBlazor.FilterContext<LogEntry> context) {
        SelectedObjectTypes = values;
        _objectTypeFilterDef.FilterFunction = x => SelectedObjectTypes.Count == 0 || (!string.IsNullOrEmpty(x.ObjectType) && SelectedObjectTypes.Contains(x.ObjectType));
        return context.Actions.ApplyFilterAsync(_objectTypeFilterDef);
    }

}
