using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Services;

namespace Inventory.Web.Configuration;

public static class ServiceRegistration
{
  public static void AddInventoryServices(this IServiceCollection services, IConfiguration configuration)
  {
    var conn = configuration.GetConnectionString("Default") ?? "Data Source=App_Data/inventory.db";
    var dbPath = conn.Replace("Data Source=", string.Empty);
    var dir = Path.GetDirectoryName(dbPath);
    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
    services.AddSingleton(new InventoryDb(conn));
    services.AddScoped<ProductService>();
    services.AddScoped<SupplierService>();
    services.AddScoped<CustomerService>();
    services.AddScoped<WarehouseService>();
    services.AddScoped<UnitService>();
    services.AddScoped<CategoryService>();
    services.AddScoped<InventoryService>();
    services.AddScoped<PurchaseService>();
    services.AddScoped<SalesService>();
    services.AddScoped<TransferService>();
    services.AddScoped<StocktakingService>();
    services.AddScoped<PurchaseReturnService>();
    services.AddScoped<SalesReturnService>();
    services.AddScoped<AuditService>();
    services.AddScoped<ICurrentUser, Inventory.Web.Services.CurrentUserAccessor>();
  }
}