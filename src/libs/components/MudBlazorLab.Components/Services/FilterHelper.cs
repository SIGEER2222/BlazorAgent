using MudBlazor;
using System.Linq.Expressions;

public static class FilterHelper<T> {
  public static Expression<Func<T, bool>> BuildExpression(List<IFilterDefinition<T>> filters) {
    if (filters == null || !filters.Any())
      return x => true;

    var expressions = filters
        .Select(filter => FilterExpressionGenerator.GenerateExpression<T>(filter, new FilterOptions()))
        .Where(expr => expr != null)
        .ToList();

    if (!expressions.Any())
      return x => true;

    var parameter = Expression.Parameter(typeof(T), "x");
    Expression combined = null;

    foreach (var expr in expressions) {
      // 替换表达式中的参数为统一的 parameter
      var replacedBody = new ParameterReplacer(expr.Parameters[0], parameter).Visit(expr.Body);
      combined = combined == null ? replacedBody : Expression.AndAlso(combined, replacedBody);
    }

    return Expression.Lambda<Func<T, bool>>(combined, parameter);
  }

  // 参数替换器，用于替换表达式中的参数
  private class ParameterReplacer : ExpressionVisitor {
    private readonly ParameterExpression _oldParameter;
    private readonly ParameterExpression _newParameter;

    public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter) {
      _oldParameter = oldParameter;
      _newParameter = newParameter;
    }

    protected override Expression VisitParameter(ParameterExpression node) {
      return node == _oldParameter ? _newParameter : base.VisitParameter(node);
    }
  }
}
