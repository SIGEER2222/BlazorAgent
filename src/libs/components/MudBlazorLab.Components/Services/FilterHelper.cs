using HmiInspection.Models;
using MudBlazor;
using System.Linq.Expressions;
using System.Reflection;

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

  public static Expression<Func<T, object>> BuildOrderSelector(string name) {
    var p = Expression.Parameter(typeof(T), "o");
    MemberInfo? member = (System.Reflection.MemberInfo?)typeof(T).GetProperty(name) ?? typeof(T).GetField(name);
    Expression prop = member is PropertyInfo pi
      ? Expression.Property(p, pi)
      : Expression.Field(p, (FieldInfo)member);
    var body = Expression.Convert(prop, typeof(object));
    return Expression.Lambda<Func<T, object>>(body, p);
  }

  public static Expression<Func<T, bool>> And(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right) {
    var parameter = Expression.Parameter(typeof(T), "x");
    var leftBody = new ParameterReplacer(left.Parameters[0], parameter).Visit(left.Body);
    var rightBody = new ParameterReplacer(right.Parameters[0], parameter).Visit(right.Body);
    var body = Expression.AndAlso(leftBody!, rightBody!);
    return Expression.Lambda<Func<T, bool>>(body, parameter);
  }
}
