using MudBlazor;
using SqlSugar;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

public enum UiAction { Insert, Update }

public class Repository<T> : SimpleClient<T>, IDisposable where T : class, new() {
  private bool disposedValue;

  public Repository(ISqlSugarClient db) {
    base.Context = db;
  }

  public async Task<List<T>> GetAllAsync() => await base.Context.Queryable<T>().ToListAsync();

  public async Task InsertOrUpdateAsync(T entity, Expression<Func<T, bool>> predicate) {
    var existing = await base.Context.Queryable<T>().AnyAsync(predicate);
    var action = existing ? UiAction.Update : UiAction.Insert;
    if (action is UiAction.Insert) {
      await base.Context.Insertable(entity).ExecuteCommandAsync();
    }
    else {
      await base.Context.Updateable(entity).Where(predicate).ExecuteCommandAsync();
    }
    await DoWhen(entity, action);
  }

  public override async Task<bool> UpdateAsync(T entity) {
    var result = await Context.Updateable(entity).ExecuteCommandAsync() > 0;
    await DoWhen(entity, UiAction.Insert);
    return result;
  }

  public override async Task<bool> InsertAsync(T entity) {
    var result = await Context.Insertable(entity).ExecuteCommandAsync() > 0;
    await DoWhen(entity, UiAction.Update);
    return result;
  }

  public virtual async Task DoWhen(T entity, UiAction action) {

  }

  public async Task InsertOrUpdateRangeAsync(List<T> entities, Expression<Func<T, object>> uniqueProperty) {
    foreach (var entity in entities) {
      var value = uniqueProperty.Compile().Invoke(entity);

      var parameter = Expression.Parameter(typeof(T), "x");
      var body = Expression.Equal(
          Expression.Invoke(uniqueProperty, parameter),
          Expression.Constant(value)
      );
      var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

      await InsertOrUpdateAsync(entity, lambda);
    }
  }

  public async Task<(List<T> items, int totalCount)> GetPageAsync(int pageIndex, int pageSize, Expression<Func<T, bool>> whereExpression = null, Expression<Func<T, object>> orderByExpression = null, bool isAsc = true) {
    var query = base.Context.Queryable<T>();

    if (whereExpression != null) {
      query = query.Where(whereExpression);
    }

    if (orderByExpression != null) {
      query = isAsc ? query.OrderBy(orderByExpression) : query.OrderBy(orderByExpression, OrderByType.Desc);
    }

    //bool isSplitTable = typeof(T).GetCustomAttribute<SugarTable>()?.IsSplitTable ?? false;

    //if (isSplitTable) {
    //  // 你要自行传入时间范围参数
    //  query = query.SplitTable(t => t.CreateTime >= start && t.CreateTime < end);
    //}

    RefAsync<int> totalCount = 0;
    var items = await query.ToPageListAsync(pageIndex, pageSize, totalCount);

    return (items, (int)totalCount);
  }

  public virtual void Dispose(bool disposing) {
    if (!disposedValue) {
      if (disposing) {
      }
      try {
        this.Context.Close();
      }
      catch (Exception ex) {
        Serilog.Log.Error(ex, "Dispose");
      }
      disposedValue = true;
    }
  }

  void IDisposable.Dispose() {
    // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }

  public async Task<GridData<TDto>> LoadGridDataAsync<TDto>(
      GridState<TDto> state,
      Func<T, TDto> selector,
      Func<string, Expression<Func<T, object>>> orderSelector,
      Expression<Func<T, bool>>? filter = null) {
    var sw = Stopwatch.StartNew();
    var sortDefinition = state.SortDefinitions.FirstOrDefault();
    Expression<Func<T, object>>? orderByExpression = null;
    bool isAsc = true;

    if (sortDefinition is not null) {
      orderByExpression = orderSelector(sortDefinition.SortBy);
      isAsc = !sortDefinition.Descending;
    }

    var (items, totalCount) = await GetPageAsync(
        pageIndex: state.Page + 1,
        pageSize: state.PageSize,
        whereExpression: filter,
        orderByExpression: orderByExpression,
        isAsc: isAsc
    );
    sw.Stop();
    if (sw.ElapsedMilliseconds > 200) {
      Serilog.Log.Warning("LoadGridData {Type} took {Elapsed}ms", typeof(T).Name, sw.ElapsedMilliseconds);
    }
    return new GridData<TDto> {
      TotalItems = totalCount,
      Items = items.Select(selector)
    };
  }
}

