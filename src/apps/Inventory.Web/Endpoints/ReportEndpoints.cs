using MiniExcelLibs;

namespace Inventory.Web.Endpoints;

  public static class ReportEndpoints
  {
    public static void MapReports(this IEndpointRouteBuilder app)
    {
    app.MapGet("/api/reports/inventory/balance", (Inventory.Infrastructure.Data.InventoryDb db, int page, int pageSize, int? warehouseId, string? productCode) =>
    {
      var q = db.Balances;
      if (warehouseId is int w) q = q.Where(x => x.WarehouseId == w);
      if (!string.IsNullOrWhiteSpace(productCode))
      {
        var p = db.Db.Queryable<Inventory.Domain.Entities.Product>().First(x => x.Code == productCode);
        if (p != null) q = q.Where(x => x.ProductId == p.Id); else q = q.Where(x => false);
      }
      int total = 0;
      var items = q.OrderBy(x => x.Id, SqlSugar.OrderByType.Desc).ToPageList(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, ref total);
      return Results.Ok(new { items, total });
    }).RequireAuthorization();

    app.MapGet("/api/reports/inventory/movements", (Inventory.Infrastructure.Data.InventoryDb db, int page, int pageSize, int? warehouseId, string? productCode, DateTime? start, DateTime? end) =>
    {
      var q = db.Movements;
      if (warehouseId is int w) q = q.Where(x => x.WarehouseId == w);
      if (!string.IsNullOrWhiteSpace(productCode))
      {
        var p = db.Db.Queryable<Inventory.Domain.Entities.Product>().First(x => x.Code == productCode);
        if (p != null) q = q.Where(x => x.ProductId == p.Id); else q = q.Where(x => false);
      }
      if (start is DateTime s) q = q.Where(x => x.OccurredAt >= s);
      if (end is DateTime e) q = q.Where(x => x.OccurredAt <= e);
      int total = 0;
      var items = q.OrderBy(x => x.OccurredAt, SqlSugar.OrderByType.Desc).ToPageList(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, ref total);
      return Results.Ok(new { items, total });
    }).RequireAuthorization();

    app.MapGet("/api/reports/sales-summary", (Inventory.Infrastructure.Data.InventoryDb db, int page, int pageSize, string? customer, DateTime? start, DateTime? end) =>
    {
      var orders = db.SalesOrders;
      if (!string.IsNullOrWhiteSpace(customer)) orders = orders.Where(x => x.CustomerCode == customer);
      if (start is DateTime s) orders = orders.Where(x => x.CreatedAt >= s);
      if (end is DateTime e) orders = orders.Where(x => x.CreatedAt <= e);
      var ids = orders.Select(x => x.Id).ToList();
      var lines = db.SalesOrderLines.Where(x => ids.Contains(x.SalesOrderId)).ToList();
      var data = lines.GroupBy(x => db.SalesOrders.First(o => o.Id == x.SalesOrderId).CustomerCode)
          .Select(g => new { Customer = g.Key, Quantity = g.Sum(x => x.Quantity), Revenue = g.Sum(x => x.Quantity * x.UnitPrice) }).ToList();
      var total = data.Count;
      var items = data.Skip(Math.Max(0, (page <= 0 ? 1 : page) - 1) * (pageSize <= 0 ? 20 : pageSize)).Take(pageSize <= 0 ? 20 : pageSize).ToList();
      return Results.Ok(new { items, total });
    }).RequireAuthorization();

    app.MapGet("/api/reports/purchase-summary", (Inventory.Infrastructure.Data.InventoryDb db, int page, int pageSize, string? supplier, DateTime? start, DateTime? end) =>
    {
      var orders = db.PurchaseOrders;
      if (!string.IsNullOrWhiteSpace(supplier)) orders = orders.Where(x => x.SupplierCode == supplier);
      if (start is DateTime s) orders = orders.Where(x => x.CreatedAt >= s);
      if (end is DateTime e) orders = orders.Where(x => x.CreatedAt <= e);
      var ids = orders.Select(x => x.Id).ToList();
      var lines = db.PurchaseOrderLines.Where(x => ids.Contains(x.PurchaseOrderId)).ToList();
      var data = lines.GroupBy(x => db.PurchaseOrders.First(o => o.Id == x.PurchaseOrderId).SupplierCode)
          .Select(g => new { Supplier = g.Key, Quantity = g.Sum(x => x.Quantity), Amount = g.Sum(x => x.Quantity * x.UnitPrice) }).ToList();
      var total = data.Count;
      var items = data.Skip(Math.Max(0, (page <= 0 ? 1 : page) - 1) * (pageSize <= 0 ? 20 : pageSize)).Take(pageSize <= 0 ? 20 : pageSize).ToList();
      return Results.Ok(new { items, total });
    }).RequireAuthorization();

    app.MapGet("/api/reports/sales-margin", (Inventory.Infrastructure.Data.InventoryDb db, int page, int pageSize, string? customer, DateTime? start, DateTime? end) =>
    {
      var orders = db.SalesOrders;
      if (!string.IsNullOrWhiteSpace(customer)) orders = orders.Where(x => x.CustomerCode == customer);
      if (start is DateTime s) orders = orders.Where(x => x.CreatedAt >= s);
      if (end is DateTime e) orders = orders.Where(x => x.CreatedAt <= e);
      var os = orders.ToList();
      var lines = db.SalesOrderLines.Where(x => os.Select(o => o.Id).Contains(x.SalesOrderId)).ToList();
      var result = new List<object>();
      foreach (var o in os)
      {
        var refCode = $"SO#{o.Code}";
        var movs = db.Movements.Where(m => m.Reference == refCode && m.Type == Inventory.Domain.Entities.MovementType.Outbound).ToList();
        var olines = lines.Where(l => l.SalesOrderId == o.Id).ToList();
        foreach (var l in olines)
        {
          var mqty = movs.Where(m => m.ProductId == l.ProductId).Sum(m => m.Quantity);
          var mcost = movs.Where(m => m.ProductId == l.ProductId).Sum(m => m.Quantity * m.UnitCost);
          var rev = l.Quantity * l.UnitPrice;
          result.Add(new { Order = o.Code, Customer = o.CustomerCode, ProductId = l.ProductId, Quantity = mqty == 0 ? l.Quantity : mqty, Revenue = rev, Cost = mcost, Margin = rev - mcost });
        }
      }
      var total = result.Count;
      var items = result.Skip(Math.Max(0, (page <= 0 ? 1 : page) - 1) * (pageSize <= 0 ? 20 : pageSize)).Take(pageSize <= 0 ? 20 : pageSize).ToList();
      return Results.Ok(new { items, total });
    }).RequireAuthorization();

    app.MapGet("/api/reports/audit", (Inventory.Infrastructure.Data.InventoryDb db, int page, int pageSize, string? user, string? operation, DateTime? start, DateTime? end) =>
    {
      var q = db.Db.Queryable<Inventory.Domain.Entities.AuditLog>();
      if (!string.IsNullOrWhiteSpace(user)) q = q.Where(x => x.User == user);
      if (!string.IsNullOrWhiteSpace(operation)) q = q.Where(x => x.Operation.Contains(operation));
      if (start is DateTime s) q = q.Where(x => x.Time >= s);
      if (end is DateTime e) q = q.Where(x => x.Time <= e);
      int total = 0;
      var items = q.OrderBy(x => x.Time, SqlSugar.OrderByType.Desc).ToPageList(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, ref total);
      return Results.Ok(new { items, total });
    }).RequireAuthorization();

      app.MapGet("/reports/inventory/balance.xlsx", (Inventory.Infrastructure.Data.InventoryDb db, int? warehouseId, string? productCode) =>
      {
        using var ms = new MemoryStream();
        var q = db.Balances;
        if (warehouseId is int w) q = q.Where(x => x.WarehouseId == w);
      if (!string.IsNullOrWhiteSpace(productCode))
      {
        var p = db.Db.Queryable<Inventory.Domain.Entities.Product>().First(x => x.Code == productCode);
        if (p != null) q = q.Where(x => x.ProductId == p.Id); else q = q.Where(x => false);
      }
      var data = q.OrderBy(x => x.Id, SqlSugar.OrderByType.Desc).ToList();
      MiniExcel.SaveAs(ms, data);
      ms.Position = 0;
      return Results.File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "inventory-balance.xlsx");
    }).RequireAuthorization();

    app.MapGet("/reports/inventory/movements.xlsx", (Inventory.Infrastructure.Data.InventoryDb db, int? warehouseId, string? productCode, DateTime? start, DateTime? end) =>
    {
      using var ms = new MemoryStream();
      var q = db.Movements;
      if (warehouseId is int w) q = q.Where(x => x.WarehouseId == w);
      if (!string.IsNullOrWhiteSpace(productCode))
      {
        var p = db.Db.Queryable<Inventory.Domain.Entities.Product>().First(x => x.Code == productCode);
        if (p != null) q = q.Where(x => x.ProductId == p.Id); else q = q.Where(x => false);
      }
      if (start is DateTime s) q = q.Where(x => x.OccurredAt >= s);
      if (end is DateTime e) q = q.Where(x => x.OccurredAt <= e);
      var data = q.OrderBy(x => x.OccurredAt, SqlSugar.OrderByType.Desc).ToList();
      MiniExcel.SaveAs(ms, data);
      ms.Position = 0;
      return Results.File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "inventory-movements.xlsx");
    }).RequireAuthorization();

    app.MapGet("/reports/sales-summary.xlsx", (Inventory.Infrastructure.Data.InventoryDb db, string? customer, DateTime? start, DateTime? end) =>
    {
      using var ms = new MemoryStream();
      var orders = db.SalesOrders;
      if (!string.IsNullOrWhiteSpace(customer)) orders = orders.Where(x => x.CustomerCode == customer);
      if (start is DateTime s) orders = orders.Where(x => x.CreatedAt >= s);
      if (end is DateTime e) orders = orders.Where(x => x.CreatedAt <= e);
      var ids = orders.Select(x => x.Id).ToList();
      var lines = db.SalesOrderLines.Where(x => ids.Contains(x.SalesOrderId)).ToList();
      var data = lines.GroupBy(x => db.SalesOrders.First(o => o.Id == x.SalesOrderId).CustomerCode)
          .Select(g => new { Customer = g.Key, Quantity = g.Sum(x => x.Quantity), Revenue = g.Sum(x => x.Quantity * x.UnitPrice) }).ToList();
      MiniExcel.SaveAs(ms, data);
      ms.Position = 0;
      return Results.File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "sales-summary.xlsx");
    }).RequireAuthorization();

    app.MapGet("/reports/purchase-summary.xlsx", (Inventory.Infrastructure.Data.InventoryDb db, string? supplier, DateTime? start, DateTime? end) =>
    {
      using var ms = new MemoryStream();
      var orders = db.PurchaseOrders;
      if (!string.IsNullOrWhiteSpace(supplier)) orders = orders.Where(x => x.SupplierCode == supplier);
      if (start is DateTime s) orders = orders.Where(x => x.CreatedAt >= s);
      if (end is DateTime e) orders = orders.Where(x => x.CreatedAt <= e);
      var ids = orders.Select(x => x.Id).ToList();
      var lines = db.PurchaseOrderLines.Where(x => ids.Contains(x.PurchaseOrderId)).ToList();
      var data = lines.GroupBy(x => db.PurchaseOrders.First(o => o.Id == x.PurchaseOrderId).SupplierCode)
          .Select(g => new { Supplier = g.Key, Quantity = g.Sum(x => x.Quantity), Amount = g.Sum(x => x.Quantity * x.UnitPrice) }).ToList();
      MiniExcel.SaveAs(ms, data);
      ms.Position = 0;
      return Results.File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "purchase-summary.xlsx");
    }).RequireAuthorization();

    app.MapGet("/reports/sales-margin.xlsx", (Inventory.Infrastructure.Data.InventoryDb db, string? customer, DateTime? start, DateTime? end) =>
    {
      using var ms = new MemoryStream();
      var orders = db.SalesOrders;
      if (!string.IsNullOrWhiteSpace(customer)) orders = orders.Where(x => x.CustomerCode == customer);
      if (start is DateTime s) orders = orders.Where(x => x.CreatedAt >= s);
      if (end is DateTime e) orders = orders.Where(x => x.CreatedAt <= e);
      var os = orders.ToList();
      var lines = db.SalesOrderLines.Where(x => os.Select(o => o.Id).Contains(x.SalesOrderId)).ToList();
      var result = new List<object>();
      foreach (var o in os)
      {
        var refCode = $"SO#{o.Code}";
        var movs = db.Movements.Where(m => m.Reference == refCode && m.Type == Inventory.Domain.Entities.MovementType.Outbound).ToList();
        var olines = lines.Where(l => l.SalesOrderId == o.Id).ToList();
        foreach (var l in olines)
        {
          var mqty = movs.Where(m => m.ProductId == l.ProductId).Sum(m => m.Quantity);
          var mcost = movs.Where(m => m.ProductId == l.ProductId).Sum(m => m.Quantity * m.UnitCost);
          var rev = l.Quantity * l.UnitPrice;
          result.Add(new { Order = o.Code, Customer = o.CustomerCode, ProductId = l.ProductId, Quantity = mqty == 0 ? l.Quantity : mqty, Revenue = rev, Cost = mcost, Margin = rev - mcost });
        }
      }
      MiniExcel.SaveAs(ms, result);
      ms.Position = 0;
      return Results.File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "sales-margin.xlsx");
    }).RequireAuthorization();

    app.MapGet("/reports/audit.xlsx", (Inventory.Infrastructure.Data.InventoryDb db, string? user, string? operation, DateTime? start, DateTime? end) =>
    {
      using var ms = new MemoryStream();
      var q = db.Db.Queryable<Inventory.Domain.Entities.AuditLog>();
      if (!string.IsNullOrWhiteSpace(user)) q = q.Where(x => x.User == user);
      if (!string.IsNullOrWhiteSpace(operation)) q = q.Where(x => x.Operation.Contains(operation));
      if (start is DateTime s) q = q.Where(x => x.Time >= s);
      if (end is DateTime e) q = q.Where(x => x.Time <= e);
      var data = q.OrderBy(x => x.Time, SqlSugar.OrderByType.Desc).ToList();
      MiniExcel.SaveAs(ms, data);
      ms.Position = 0;
      return Results.File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "audit.xlsx");
    }).RequireAuthorization();
  }
}