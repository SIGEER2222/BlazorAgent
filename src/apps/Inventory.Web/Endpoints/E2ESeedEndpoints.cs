namespace Inventory.Web.Endpoints;

public static class E2ESeedEndpoints
{
  public static void Map(this IEndpointRouteBuilder app)
  {
    app.MapPost("/e2e/seed/purchase", async (HttpContext ctx, Inventory.Infrastructure.Data.InventoryDb db, Inventory.Infrastructure.Services.PurchaseService purchaseSvc) =>
    {
      var principal = MudBlazorLab.Components.Services.UserService.SignIn("manager@example.com", "P@ssw0rd!");
      if (principal != null)
      {
        ctx.User = principal;
        await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignInAsync(ctx, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, principal);
      }
      var code = $"E2EPO-{DateTime.UtcNow.Ticks}";
      var po = new Inventory.Domain.Entities.PurchaseOrder { Code = code, SupplierCode = "SUP01", Remark = "E2E seed", Status = Inventory.Domain.Entities.PurchaseOrderStatus.Draft };
      await purchaseSvc.CreateAsync(po);
      var pid = db.Db.Queryable<Inventory.Domain.Entities.Product>().First(x => x.Code == "P002")!.Id;
      var whId = db.Db.Queryable<Inventory.Domain.Entities.Warehouse>().First(x => x.Code == "WH1")!.Id;
      await purchaseSvc.AddLineAsync(new Inventory.Domain.Entities.PurchaseOrderLine { PurchaseOrderId = po.Id, ProductId = pid, WarehouseId = whId, Quantity = 5, UnitPrice = 10 });
      await purchaseSvc.ApproveAsync(po.Id);
      await purchaseSvc.ReceiveAsync(po.Id);
      return Results.Ok(new { code });
    }).DisableAntiforgery();

    app.MapPost("/e2e/seed/sales", async (HttpContext ctx, Inventory.Infrastructure.Data.InventoryDb db, Inventory.Infrastructure.Services.SalesService salesSvc) =>
    {
      var principal = MudBlazorLab.Components.Services.UserService.SignIn("manager@example.com", "P@ssw0rd!");
      if (principal != null)
      {
        ctx.User = principal;
        await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignInAsync(ctx, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, principal);
      }
      var code = $"E2ESO-{DateTime.UtcNow.Ticks}";
      var so = new Inventory.Domain.Entities.SalesOrder { Code = code, CustomerCode = "CUST01", Remark = "E2E seed", Status = Inventory.Domain.Entities.SalesOrderStatus.Draft };
      await salesSvc.CreateAsync(so);
      var pid = db.Db.Queryable<Inventory.Domain.Entities.Product>().First(x => x.Code == "P002")!.Id;
      var whId = db.Db.Queryable<Inventory.Domain.Entities.Warehouse>().First(x => x.Code == "WH1")!.Id;
      await salesSvc.AddLineAsync(new Inventory.Domain.Entities.SalesOrderLine { SalesOrderId = so.Id, ProductId = pid, WarehouseId = whId, Quantity = 3, UnitPrice = 20 });
      await salesSvc.ApproveAsync(so.Id);
      await salesSvc.ShipAsync(so.Id);
      return Results.Ok(new { code });
    }).DisableAntiforgery();
  }
}