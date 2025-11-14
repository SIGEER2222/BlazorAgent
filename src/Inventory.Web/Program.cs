using MudBlazor.Services;
using MudBlazorLab.Components.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IPermissionService>(new PermissionService(PermissionRegistry.FeatureRoles));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.RequireAdmin, policy => policy.RequireRole("Admin"));
    options.AddPolicy(AuthPolicies.RequireManagerOrAdmin, policy => policy.RequireRole("Manager", "Admin"));
    options.AddPolicy(AuthPolicies.RequireEditor, policy => policy.RequireRole("Editor"));
});

builder.Services.AddHttpContextAccessor();

var conn = builder.Configuration.GetConnectionString("Default") ?? "Data Source=App_Data/inventory.db";
var dbPath = conn.Replace("Data Source=", string.Empty);
var dir = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
builder.Services.AddSingleton(new InventoryDb(conn));
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<WarehouseService>();
builder.Services.AddScoped<UnitService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<PurchaseService>();
builder.Services.AddScoped<SalesService>();
builder.Services.AddScoped<TransferService>();
builder.Services.AddScoped<StocktakingService>();
builder.Services.AddScoped<PurchaseReturnService>();
builder.Services.AddScoped<SalesReturnService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<ICurrentUser, Inventory.Web.Services.CurrentUserAccessor>();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  app.UseHsts();
}

var httpsPort = app.Configuration["ASPNETCORE_HTTPS_PORT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT");
if (!string.IsNullOrEmpty(httpsPort))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<Inventory.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/auth/login", async (HttpContext ctx, LoginDto dto) =>
{
    var principal = UserService.SignIn(dto.Username, dto.Password);
    if (principal == null) return Results.Unauthorized();
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    return Results.Ok(new { name = principal.Identity!.Name });
}).DisableAntiforgery();

app.MapPost("/auth/login-form", async (HttpContext ctx) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var username = form["Username"].ToString();
    var password = form["Password"].ToString();
    var principal = UserService.SignIn(username, password);
    if (principal == null) return Results.Redirect("/login?error=1");
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    return Results.Redirect("/");
}).DisableAntiforgery();

app.MapPost("/auth/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
}).DisableAntiforgery();

app.MapGet("/health", () => Results.Ok(new { status = "OK" }));

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
    var data = q.OrderBy(x => x.Id).ToList();
    MiniExcelLibs.MiniExcel.SaveAs(ms, data);
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
    var data = q.OrderBy(x => x.Id).ToList();
    MiniExcelLibs.MiniExcel.SaveAs(ms, data);
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
    MiniExcelLibs.MiniExcel.SaveAs(ms, data);
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
    MiniExcelLibs.MiniExcel.SaveAs(ms, data);
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
    MiniExcelLibs.MiniExcel.SaveAs(ms, result);
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
    MiniExcelLibs.MiniExcel.SaveAs(ms, data);
    ms.Position = 0;
    return Results.File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "audit.xlsx");
}).RequireAuthorization();

// seed sample data for tests and demo
void SeedSampleData(Inventory.Infrastructure.Data.InventoryDb db)
{
    var wh1 = db.Warehouses.First(x => x.Code == "WH1");
    if (wh1 == null)
    {
        db.Db.Insertable(new Inventory.Domain.Entities.Warehouse { Code = "WH1", Name = "主仓" }).ExecuteCommand();
        wh1 = db.Warehouses.First(x => x.Code == "WH1");
    }

    var sup01 = db.Suppliers.First(x => x.Code == "SUP01");
    if (sup01 == null)
    {
        db.Db.Insertable(new Inventory.Domain.Entities.Supplier { Code = "SUP01", Name = "示例供应商", Contact = "张三", Phone = "000-0000", Email = "sup01@example.com", Address = "示例地址" }).ExecuteCommand();
    }

    var cust01 = db.Customers.First(x => x.Code == "CUST01");
    if (cust01 == null)
    {
        db.Db.Insertable(new Inventory.Domain.Entities.Customer { Code = "CUST01", Name = "示例客户", Contact = "李四", Phone = "000-0001", Email = "cust01@example.com", Address = "示例地址" }).ExecuteCommand();
    }

    var unitPcs = db.Units.First(x => x.Name == "PCS");
    if (unitPcs == null)
    {
        db.Db.Insertable(new Inventory.Domain.Entities.Unit { Name = "PCS", Symbol = "件" }).ExecuteCommand();
    }

    var catGeneral = db.Categories.First(x => x.Name == "General");
    if (catGeneral == null)
    {
        db.Db.Insertable(new Inventory.Domain.Entities.Category { Name = "General" }).ExecuteCommand();
    }

    void EnsureProduct(string code, string name)
    {
        var p = db.Products.First(x => x.Code == code);
        if (p == null)
        {
            db.Db.Insertable(new Inventory.Domain.Entities.Product { Code = code, Name = name, Unit = "PCS", Category = "General" }).ExecuteCommand();
        }
    }

    EnsureProduct("P001", "示例产品1");
    EnsureProduct("P002", "示例产品2");
    EnsureProduct("P003", "示例产品3");

    int GetProductId(string code) => db.Db.Queryable<Inventory.Domain.Entities.Product>().First(x => x.Code == code)!.Id;
    int whId = db.Db.Queryable<Inventory.Domain.Entities.Warehouse>().First(x => x.Code == "WH1")!.Id;

    void EnsureBalance(string pcode, decimal qty, decimal cost)
    {
        var pid = GetProductId(pcode);
        var bal = db.Db.Queryable<Inventory.Domain.Entities.StockBalance>().First(x => x.WarehouseId == whId && x.ProductId == pid);
        if (bal == null)
        {
            db.Db.Insertable(new Inventory.Domain.Entities.StockBalance { WarehouseId = whId, ProductId = pid, Quantity = qty, AvgCost = cost }).ExecuteCommand();
        }
    }

    // ensure some stock for shipment and stocktake tests
    EnsureBalance("P002", 100, 10);
    EnsureBalance("P003", 50, 8);
}

var dbForSeed = app.Services.GetRequiredService<Inventory.Infrastructure.Data.InventoryDb>();
SeedSampleData(dbForSeed);

app.Run();

public record LoginDto(string Username, string Password);
