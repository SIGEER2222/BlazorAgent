using Inventory.Domain.Entities;
using Inventory.Infrastructure.Services;

namespace Inventory.Web.Endpoints;

public static class ProcessEndpoints
{
  public static void MapProcess(this IEndpointRouteBuilder app)
  {
    var api = app.MapGroup("/api").RequireAuthorization();

    api.MapGet("/purchase-orders", async (PurchaseService svc, int page = 1, int pageSize = 20, string? keyword = null) =>
    {
      var (items, total) = await svc.QueryAsync(page, pageSize, keyword);
      return Results.Ok(new { items, total });
    });

    api.MapPost("/purchase-orders", async (PurchaseService svc, PurchaseOrder po) =>
    {
      po.Code ??= string.Empty; po.SupplierCode ??= string.Empty; po.Remark ??= string.Empty;
      var id = await svc.CreateAsync(po);
      return Results.Created($"/api/purchase-orders/{id}", new { id });
    }).DisableAntiforgery();

    api.MapGet("/purchase-orders/{id:int}", (Inventory.Infrastructure.Data.InventoryDb db, int id) =>
    {
      var po = db.Db.Queryable<PurchaseOrder>().First(x => x.Id == id);
      return po is null ? Results.NotFound() : Results.Ok(po);
    });

    api.MapGet("/purchase-orders/{id:int}/lines", async (PurchaseService svc, int id) =>
    {
      var lines = await svc.GetLinesAsync(id);
      return Results.Ok(lines);
    });

    api.MapPost("/purchase-orders/{id:int}/lines", async (PurchaseService svc, int id, PurchaseOrderLine line) =>
    {
      line.PurchaseOrderId = id;
      await svc.AddLineAsync(line);
      return Results.Created($"/api/purchase-orders/{id}/lines", new { id = line.Id });
    }).DisableAntiforgery();

    api.MapPost("/purchase-orders/{id:int}/approve", async (PurchaseService svc, int id) =>
    {
      await svc.ApproveAsync(id);
      return Results.NoContent();
    }).DisableAntiforgery();

    api.MapPost("/purchase-orders/{id:int}/receive", async (PurchaseService svc, int id) =>
    {
      await svc.ReceiveAsync(id);
      return Results.NoContent();
    }).DisableAntiforgery();

    api.MapDelete("/purchase-orders/{id:int}", async (PurchaseService svc, int id) =>
    {
      var ok = await svc.DeleteAsync(id);
      return ok ? Results.NoContent() : Results.BadRequest();
    }).DisableAntiforgery();

    api.MapGet("/sales-orders", async (SalesService svc, int page = 1, int pageSize = 20, string? keyword = null) =>
    {
      var (items, total) = await svc.QueryAsync(page, pageSize, keyword);
      return Results.Ok(new { items, total });
    });

    api.MapPost("/sales-orders", async (SalesService svc, SalesOrder so) =>
    {
      so.Code ??= string.Empty; so.CustomerCode ??= string.Empty; so.Remark ??= string.Empty;
      var id = await svc.CreateAsync(so);
      return Results.Created($"/api/sales-orders/{id}", new { id });
    }).DisableAntiforgery();

    api.MapGet("/sales-orders/{id:int}", (Inventory.Infrastructure.Data.InventoryDb db, int id) =>
    {
      var so = db.Db.Queryable<SalesOrder>().First(x => x.Id == id);
      return so is null ? Results.NotFound() : Results.Ok(so);
    });

    api.MapGet("/sales-orders/{id:int}/lines", async (SalesService svc, int id) =>
    {
      var lines = await svc.GetLinesAsync(id);
      return Results.Ok(lines);
    });

    api.MapPost("/sales-orders/{id:int}/lines", async (SalesService svc, int id, SalesOrderLine line) =>
    {
      line.SalesOrderId = id;
      await svc.AddLineAsync(line);
      return Results.Created($"/api/sales-orders/{id}/lines", new { id = line.Id });
    }).DisableAntiforgery();

    api.MapPost("/sales-orders/{id:int}/approve", async (SalesService svc, int id) =>
    {
      await svc.ApproveAsync(id);
      return Results.NoContent();
    }).DisableAntiforgery();

    api.MapPost("/sales-orders/{id:int}/ship", async (SalesService svc, int id) =>
    {
      var ok = await svc.ShipAsync(id);
      return ok ? Results.NoContent() : Results.BadRequest();
    }).DisableAntiforgery();

    api.MapDelete("/sales-orders/{id:int}", async (SalesService svc, int id) =>
    {
      var ok = await svc.DeleteAsync(id);
      return ok ? Results.NoContent() : Results.BadRequest();
    }).DisableAntiforgery();

    api.MapGet("/purchase-returns", async (PurchaseReturnService svc, int page = 1, int pageSize = 20, string? keyword = null) =>
    {
      var (items, total) = await svc.QueryAsync(page, pageSize, keyword);
      return Results.Ok(new { items, total });
    });

    api.MapPost("/purchase-returns", async (PurchaseReturnService svc, PurchaseReturn r) =>
    {
      r.Code ??= string.Empty; r.SupplierCode ??= string.Empty; r.Remark ??= string.Empty;
      var id = await svc.CreateAsync(r);
      return Results.Created($"/api/purchase-returns/{id}", new { id });
    }).DisableAntiforgery();

    api.MapGet("/purchase-returns/{id:int}", (Inventory.Infrastructure.Data.InventoryDb db, int id) =>
    {
      var r = db.Db.Queryable<PurchaseReturn>().First(x => x.Id == id);
      return r is null ? Results.NotFound() : Results.Ok(r);
    });

    api.MapGet("/purchase-returns/{id:int}/lines", async (PurchaseReturnService svc, int id) =>
    {
      var lines = await svc.GetLinesAsync(id);
      return Results.Ok(lines);
    });

    api.MapPost("/purchase-returns/{id:int}/lines", async (PurchaseReturnService svc, int id, PurchaseReturnLine line) =>
    {
      line.PurchaseReturnId = id;
      await svc.AddLineAsync(line);
      return Results.Created($"/api/purchase-returns/{id}/lines", new { id = line.Id });
    }).DisableAntiforgery();

    api.MapPost("/purchase-returns/{id:int}/process", async (PurchaseReturnService svc, int id) =>
    {
      var ok = await svc.ProcessAsync(id);
      return ok ? Results.NoContent() : Results.BadRequest();
    }).DisableAntiforgery();

    api.MapDelete("/purchase-returns/{id:int}", async (PurchaseReturnService svc, int id) =>
    {
      var ok = await svc.DeleteAsync(id);
      return ok ? Results.NoContent() : Results.BadRequest();
    }).DisableAntiforgery();

    api.MapGet("/sales-returns", async (SalesReturnService svc, int page = 1, int pageSize = 20, string? keyword = null) =>
    {
      var (items, total) = await svc.QueryAsync(page, pageSize, keyword);
      return Results.Ok(new { items, total });
    });

    api.MapPost("/sales-returns", async (SalesReturnService svc, SalesReturn r) =>
    {
      r.Code ??= string.Empty; r.CustomerCode ??= string.Empty; r.Remark ??= string.Empty;
      var id = await svc.CreateAsync(r);
      return Results.Created($"/api/sales-returns/{id}", new { id });
    }).DisableAntiforgery();

    api.MapGet("/sales-returns/{id:int}", (Inventory.Infrastructure.Data.InventoryDb db, int id) =>
    {
      var r = db.Db.Queryable<SalesReturn>().First(x => x.Id == id);
      return r is null ? Results.NotFound() : Results.Ok(r);
    });

    api.MapGet("/sales-returns/{id:int}/lines", async (SalesReturnService svc, int id) =>
    {
      var lines = await svc.GetLinesAsync(id);
      return Results.Ok(lines);
    });

    api.MapPost("/sales-returns/{id:int}/lines", async (SalesReturnService svc, int id, SalesReturnLine line) =>
    {
      line.SalesReturnId = id;
      await svc.AddLineAsync(line);
      return Results.Created($"/api/sales-returns/{id}/lines", new { id = line.Id });
    }).DisableAntiforgery();

    api.MapPost("/sales-returns/{id:int}/process", async (SalesReturnService svc, int id) =>
    {
      var ok = await svc.ProcessAsync(id);
      return ok ? Results.NoContent() : Results.BadRequest();
    }).DisableAntiforgery();

    api.MapDelete("/sales-returns/{id:int}", async (SalesReturnService svc, int id) =>
    {
      var ok = await svc.DeleteAsync(id);
      return ok ? Results.NoContent() : Results.BadRequest();
    }).DisableAntiforgery();

    api.MapGet("/stocktakes", async (StocktakingService svc, int page = 1, int pageSize = 20, string? keyword = null) =>
    {
      var (items, total) = await svc.QueryAsync(page, pageSize, keyword);
      return Results.Ok(new { items, total });
    });

    api.MapPost("/stocktakes", async (StocktakingService svc, StockCount sc) =>
    {
      sc.Code ??= string.Empty;
      var id = await svc.CreateAsync(sc);
      return Results.Created($"/api/stocktakes/{id}", new { id });
    }).DisableAntiforgery();

    api.MapGet("/stocktakes/{id:int}", (Inventory.Infrastructure.Data.InventoryDb db, int id) =>
    {
      var sc = db.Db.Queryable<StockCount>().First(x => x.Id == id);
      return sc is null ? Results.NotFound() : Results.Ok(sc);
    });

    api.MapGet("/stocktakes/{id:int}/lines", async (StocktakingService svc, int id) =>
    {
      var lines = await svc.GetLinesAsync(id);
      return Results.Ok(lines);
    });

    api.MapPost("/stocktakes/{id:int}/lines", async (StocktakingService svc, int id, StockCountLine line) =>
    {
      line.StockCountId = id;
      await svc.AddLineAsync(line);
      return Results.Created($"/api/stocktakes/{id}/lines", new { id = line.Id });
    }).DisableAntiforgery();

    api.MapPost("/stocktakes/{id:int}/apply", async (StocktakingService svc, int id) =>
    {
      await svc.ApplyAsync(id);
      return Results.NoContent();
    }).DisableAntiforgery();

    api.MapPost("/transfers", async (TransferService svc, TransferDto dto) =>
    {
      var ok = await svc.TransferAsync(dto.FromWarehouseId, dto.ToWarehouseId, dto.ProductId, dto.Qty, dto.UnitCost, dto.Reference ?? string.Empty);
      return ok ? Results.NoContent() : Results.BadRequest();
    }).DisableAntiforgery();
  }

  public class TransferDto
  {
    public int FromWarehouseId { get; set; }
    public int ToWarehouseId { get; set; }
    public int ProductId { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitCost { get; set; }
    public string? Reference { get; set; }
  }
}
