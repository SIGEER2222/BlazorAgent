using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MudBlazorLab.Components.Components;

public partial class AutoDataGrid<TItem> : ComponentBase
{
    [Parameter] public IEnumerable<TItem>? Source { get; set; }
    [Parameter] public string Title { get; set; } = "数据表格";
    [Parameter] public bool Dense { get; set; } = true;
    [Parameter] public bool Bordered { get; set; } = true;
    [Parameter] public bool Hover { get; set; } = true;
    [Parameter] public bool Filterable { get; set; } = true;
    [Parameter] public bool FixedHeader { get; set; } = true;
    [Parameter] public string Height { get; set; } = "70vh";
    [Parameter] public ResizeMode ColumnResizeMode { get; set; } = ResizeMode.Container;
    [Parameter] public Func<GridState<TItem>, Task<GridData<TItem>>>? ServerData { get; set; }
    [Parameter] public Func<Type, object?, string>? EnumFormatter { get; set; }

    IReadOnlyList<PropertyInfo> DisplayProps => typeof(TItem)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(p => p.CanRead)
        .ToList();

    IEnumerable<TItem> ViewSource => ApplyEnumFilters(Source ?? Enumerable.Empty<TItem>());

    readonly Dictionary<string, HashSet<string>> _enumSelected = new();

    Expression<Func<TItem, TProp>> GetLambda<TProp>(PropertyInfo p)
    {
        var param = Expression.Parameter(typeof(TItem), "x");
        var access = Expression.Property(param, p);
        Expression body = access;
        if (access.Type != typeof(TProp))
            body = Expression.Convert(access, typeof(TProp));
        return Expression.Lambda<Func<TItem, TProp>>(body, param);
    }

    bool IsEnum(PropertyInfo p)
        => (Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType).IsEnum;

    string EnumText(TItem row, PropertyInfo p)
    {
        var v = p.GetValue(row);
        var enumType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
        if (v == null) return string.Empty;
        if (EnumFormatter != null) return EnumFormatter(enumType, v);
        return GetEnumDisplay(enumType, v);
    }

    string GetEnumDisplay(Type enumType, object value)
    {
        try
        {
            var name = Enum.GetName(enumType, value);
            if (name == null) return value.ToString() ?? string.Empty;
            var field = enumType.GetField(name);
            if (field != null)
            {
                var disp = field.GetCustomAttribute<DisplayAttribute>();
                if (disp != null && !string.IsNullOrWhiteSpace(disp.Name)) return disp.Name!;
                var desc = field.GetCustomAttribute<DescriptionAttribute>();
                if (desc != null && !string.IsNullOrWhiteSpace(desc.Description)) return desc.Description!;
            }
            return name;
        }
        catch
        {
            return value.ToString() ?? string.Empty;
        }
    }

    IEnumerable<(string Key, string Text)> GetEnumOptions(PropertyInfo p)
    {
        var enumType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
        foreach (var val in Enum.GetValues(enumType))
        {
            var key = Convert.ToInt64(val).ToString();
            var text = GetEnumDisplay(enumType, val);
            yield return (key, text);
        }
    }

    IList<string> GetEnumSelectedKeys(PropertyInfo p)
    {
        var key = p.Name;
        if (_enumSelected.TryGetValue(key, out var set)) return set.ToList();
        return Array.Empty<string>();
    }

    void OnEnumSelected(PropertyInfo p, IEnumerable<string> keys)
    {
        var key = p.Name;
        _enumSelected[key] = new HashSet<string>(keys ?? Array.Empty<string>());
        StateHasChanged();
    }

    

    readonly Dictionary<string, bool> _enumFilterOpen = new();
    bool IsEnumFilterOpen(PropertyInfo p) => _enumFilterOpen.TryGetValue(p.Name, out var v) && v;
    void SetEnumFilterOpen(PropertyInfo p, bool open) => _enumFilterOpen[p.Name] = open;
    bool IsEnumSelected(PropertyInfo p, string key) => _enumSelected.TryGetValue(p.Name, out var set) && set.Contains(key);
    bool IsEnumSelectAll(PropertyInfo p)
    {
        var all = GetEnumOptions(p).Select(o => o.Key).ToHashSet();
        if (!_enumSelected.TryGetValue(p.Name, out var set)) return false;
        return set.IsSupersetOf(all);
    }
    void SelectAllEnum(PropertyInfo p, bool value)
    {
        if (value)
            _enumSelected[p.Name] = GetEnumOptions(p).Select(o => o.Key).ToHashSet();
        else
            _enumSelected[p.Name] = new HashSet<string>();
        StateHasChanged();
    }
    void OnEnumCheckboxChanged(PropertyInfo p, string key, bool value)
    {
        if (!_enumSelected.TryGetValue(p.Name, out var set))
        {
            set = new HashSet<string>();
            _enumSelected[p.Name] = set;
        }
        if (value) set.Add(key); else set.Remove(key);
        StateHasChanged();
    }

    IEnumerable<TItem> ApplyEnumFilters(IEnumerable<TItem> source)
    {
        var query = source;
        foreach (var kv in _enumSelected)
        {
            if (kv.Value == null || kv.Value.Count == 0) continue;
            var p = DisplayProps.FirstOrDefault(x => x.Name == kv.Key);
            if (p == null) continue;
            var enumType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            var selectedLongs = kv.Value.Select(v => long.Parse(v)).ToHashSet();
            query = query.Where(item =>
            {
                var v = p.GetValue(item);
                if (v == null) return false;
                var lv = Convert.ToInt64(v);
                return selectedLongs.Contains(lv);
            });
        }
        return query;
    }
}
