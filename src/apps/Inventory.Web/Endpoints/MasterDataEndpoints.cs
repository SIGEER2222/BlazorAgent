using Inventory.Domain.Entities;
using Inventory.Infrastructure.Services;

namespace Inventory.Web.Endpoints;

public static class MasterDataEndpoints
{
  public static void MapMasterData(this IEndpointRouteBuilder app)
  {
    var api = app.MapGroup("/api").RequireAuthorization();

    api.MapGet("/products", async (ProductService svc, int page = 1, int pageSize = 20, string? keyword = null) =>
    {
      var (items, total) = await svc.QueryAsync(page, pageSize, keyword);
      return Results.Ok(new { items, total });
    });

    api.MapGet("/products/{id:int}", async (ProductService svc, int id) =>
    {
      var p = await svc.GetAsync(id);
      return p is null ? Results.NotFound() : Results.Ok(p);
    });

    api.MapPost("/products", async (ProductService svc, Product p) =>
    {
      p.Code ??= string.Empty; p.Name ??= string.Empty; p.Category ??= p.Category ?? string.Empty; p.Unit ??= p.Unit ?? string.Empty;
      var id = await svc.CreateAsync(p);
      return Results.Created($"/api/products/{id}", new { id });
    }).DisableAntiforgery();

    api.MapPut("/products/{id:int}", async (ProductService svc, int id, Product p) =>
    {
      p.Id = id; p.Code ??= string.Empty; p.Name ??= string.Empty; p.Category ??= p.Category ?? string.Empty; p.Unit ??= p.Unit ?? string.Empty;
      await svc.UpdateAsync(p);
      return Results.NoContent();
    }).DisableAntiforgery();

    api.MapDelete("/products/{id:int}", async (ProductService svc, int id) =>
    {
      await svc.DeleteAsync(id);
      return Results.NoContent();
    }).DisableAntiforgery();

    api.MapGet("/customers", async (CustomerService svc, int page = 1, int pageSize = 20, string? keyword = null) =>
    {
      var (items, total) = await svc.QueryAsync(page, pageSize, keyword);
      return Results.Ok(new { items, total });
    });

    api.MapGet("/customers/{id:int}", async (CustomerService svc, int id) =>
    {
      var p = await svc.GetAsync(id);
      return p is null ? Results.NotFound() : Results.Ok(p);
    });

    api.MapPost("/customers", async (CustomerService svc, Customer p) =>
    {
      p.Code ??= string.Empty; p.Name ??= string.Empty; p.Contact ??= string.Empty; p.Phone ??= string.Empty; p.Email ??= string.Empty; p.Address ??= string.Empty;
      var id = await svc.CreateAsync(p);
      return Results.Created($"/api/customers/{id}", new { id });
    }).DisableAntiforgery();

    api.MapPut("/customers/{id:int}", async (CustomerService svc, int id, Customer p) =>
    {
      p.Id = id; p.Code ??= string.Empty; p.Name ??= string.Empty; p.Contact ??= string.Empty; p.Phone ??= string.Empty; p.Email ??= string.Empty; p.Address ??= string.Empty;
      await svc.UpdateAsync(p);
      return Results.NoContent();
    }).DisableAntiforgery();

    api.MapDelete("/customers/{id:int}", async (CustomerService svc, int id) =>
    {
      await svc.DeleteAsync(id);
      return Results.NoContent();
    }).DisableAntiforgery();

    api.MapGet("/suppliers", async (SupplierService svc, int page = 1, int pageSize = 20, string? keyword = null) =>
    {
      var (items, total) = await svc.QueryAsync(page, pageSize, keyword);
      return Results.Ok(new { items, total });
    });

    api.MapGet("/suppliers/{id:int}", async (SupplierService svc, int id) =>
    {
      var p = await svc.GetAsync(id);
      return p is null ? Results.NotFound() : Results.Ok(p);
    });

    api.MapPost("/suppliers", async (SupplierService svc, Supplier p) =>
    {
      p.Code ??= string.Empty; p.Name ??= string.Empty; p.Contact ??= string.Empty; p.Phone ??= string.Empty; p.Email ??= string.Empty; p.Address ??= string.Empty;
      var id = await svc.CreateAsync(p);
      return Results.Created($"/api/suppliers/{id}", new { id });
    }).DisableAntiforgery();

    api.MapPut("/suppliers/{id:int}", async (SupplierService svc, int id, Supplier p) =>
    {
      p.Id = id; p.Code ??= string.Empty; p.Name ??= string.Empty; p.Contact ??= string.Empty; p.Phone ??= string.Empty; p.Email ??= string.Empty; p.Address ??= string.Empty;
      await svc.UpdateAsync(p);
      return Results.NoContent();
    }).DisableAntiforgery();

    api.MapDelete("/suppliers/{id:int}", async (SupplierService svc, int id) =>
    {
      await svc.DeleteAsync(id);
      return Results.NoContent();
    }).DisableAntiforgery();

    api.MapGet("/warehouses", async (WarehouseService svc, int page = 1, int pageSize = 20, string? keyword = null) =>
    {
      var (items, total) = await svc.QueryAsync(page, pageSize, keyword);
      return Results.Ok(new { items, total });
    });

    api.MapGet("/warehouses/{id:int}", async (WarehouseService svc, int id) =>
    {
      var p = await svc.GetAsync(id);
      return p is null ? Results.NotFound() : Results.Ok(p);
    });

    api.MapPost("/warehouses", async (WarehouseService svc, Warehouse p) =>
    {
      p.Code ??= string.Empty; p.Name ??= string.Empty; p.Location ??= string.Empty;
      var id = await svc.CreateAsync(p);
      return Results.Created($"/api/warehouses/{id}", new { id });
    }).DisableAntiforgery();

    api.MapPut("/warehouses/{id:int}", async (WarehouseService svc, int id, Warehouse p) =>
    {
      p.Id = id; p.Code ??= string.Empty; p.Name ??= string.Empty; p.Location ??= string.Empty;
      await svc.UpdateAsync(p);
      return Results.NoContent();
    }).DisableAntiforgery();

    api.MapDelete("/warehouses/{id:int}", async (WarehouseService svc, int id) =>
    {
      await svc.DeleteAsync(id);
      return Results.NoContent();
    }).DisableAntiforgery();

    api.MapGet("/units", async (UnitService svc, int page = 1, int pageSize = 20, string? keyword = null) =>
    {
      var (items, total) = await svc.QueryAsync(page, pageSize, keyword);
      return Results.Ok(new { items, total });
    });

    api.MapGet("/units/{id:int}", async (UnitService svc, int id) =>
    {
      var p = await svc.GetAsync(id);
      return p is null ? Results.NotFound() : Results.Ok(p);
    });

    api.MapPost("/units", async (UnitService svc, Unit p) =>
    {
      p.Name ??= string.Empty; p.Symbol ??= string.Empty;
      var id = await svc.CreateAsync(p);
      return Results.Created($"/api/units/{id}", new { id });
    }).DisableAntiforgery();

    api.MapPut("/units/{id:int}", async (UnitService svc, int id, Unit p) =>
    {
      p.Id = id; p.Name ??= string.Empty; p.Symbol ??= string.Empty;
      await svc.UpdateAsync(p);
      return Results.NoContent();
    }).DisableAntiforgery();

    api.MapDelete("/units/{id:int}", async (UnitService svc, int id) =>
    {
      await svc.DeleteAsync(id);
      return Results.NoContent();
    }).DisableAntiforgery();

    api.MapGet("/categories", async (CategoryService svc, int page = 1, int pageSize = 20, string? keyword = null) =>
    {
      var (items, total) = await svc.QueryAsync(page, pageSize, keyword);
      return Results.Ok(new { items, total });
    });

    api.MapGet("/categories/{id:int}", async (CategoryService svc, int id) =>
    {
      var p = await svc.GetAsync(id);
      return p is null ? Results.NotFound() : Results.Ok(p);
    });

    api.MapPost("/categories", async (CategoryService svc, Category p) =>
    {
      p.Name ??= string.Empty;
      var id = await svc.CreateAsync(p);
      return Results.Created($"/api/categories/{id}", new { id });
    }).DisableAntiforgery();

    api.MapPut("/categories/{id:int}", async (CategoryService svc, int id, Category p) =>
    {
      p.Id = id; p.Name ??= string.Empty;
      await svc.UpdateAsync(p);
      return Results.NoContent();
    }).DisableAntiforgery();

    api.MapDelete("/categories/{id:int}", async (CategoryService svc, int id) =>
    {
      await svc.DeleteAsync(id);
      return Results.NoContent();
    }).DisableAntiforgery();
  }
}