using MudBlazor;
using SqlSugar;
using System.Reflection;

namespace InspectionSystem.Services;

public static class GridServerData
{
    public static async Task<GridData<T>> QueryAsync<T>(ISugarQueryable<T> source, GridState<T> state)
    {
        var page = state.Page + 1;
        var size = state.PageSize;

        // default sort by TxnTime desc if exists
        var prop = typeof(T).GetProperty("TxnTime", BindingFlags.Public | BindingFlags.Instance);
        if (prop != null)
        {
            source = source.OrderBy("TxnTime desc");
        }

        int total = 0;
        var items = source.ToPageList(page, size, ref total);
        return new GridData<T> { Items = items, TotalItems = total };
    }
}
