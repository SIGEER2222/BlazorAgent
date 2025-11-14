using SqlSugar;
using Inventory.Domain.Entities;

namespace Inventory.Infrastructure.Data;

public class InventoryDb
{
    public SqlSugarClient Db { get; }

    public InventoryDb(string connectionString)
    {
        Db = new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
        });

        Db.CodeFirst.InitTables(typeof(Product), typeof(Supplier), typeof(Customer), typeof(Warehouse), typeof(Unit), typeof(Category), typeof(PurchaseOrder), typeof(PurchaseOrderLine), typeof(SalesOrder), typeof(SalesOrderLine), typeof(InventoryMovement), typeof(StockBalance), typeof(StockCount), typeof(StockCountLine), typeof(PurchaseReturn), typeof(PurchaseReturnLine), typeof(SalesReturn), typeof(SalesReturnLine), typeof(AuditLog));
        Db.QueryFilter.AddTableFilter<Product>(p => p.Enabled == true);
        Db.QueryFilter.AddTableFilter<Supplier>(p => p.Enabled == true);
        Db.QueryFilter.AddTableFilter<Customer>(p => p.Enabled == true);
        Db.QueryFilter.AddTableFilter<Warehouse>(p => p.Enabled == true);
        Db.QueryFilter.AddTableFilter<Unit>(p => p.Enabled == true);
        Db.QueryFilter.AddTableFilter<Category>(p => p.Enabled == true);
    }

    public ISugarQueryable<Product> Products => Db.Queryable<Product>();
    public ISugarQueryable<Supplier> Suppliers => Db.Queryable<Supplier>();
    public ISugarQueryable<Customer> Customers => Db.Queryable<Customer>();
    public ISugarQueryable<Warehouse> Warehouses => Db.Queryable<Warehouse>();
    public ISugarQueryable<Unit> Units => Db.Queryable<Unit>();
    public ISugarQueryable<Category> Categories => Db.Queryable<Category>();
    public ISugarQueryable<PurchaseOrder> PurchaseOrders => Db.Queryable<PurchaseOrder>();
    public ISugarQueryable<PurchaseOrderLine> PurchaseOrderLines => Db.Queryable<PurchaseOrderLine>();
    public ISugarQueryable<SalesOrder> SalesOrders => Db.Queryable<SalesOrder>();
    public ISugarQueryable<SalesOrderLine> SalesOrderLines => Db.Queryable<SalesOrderLine>();
    public ISugarQueryable<InventoryMovement> Movements => Db.Queryable<InventoryMovement>();
    public ISugarQueryable<StockBalance> Balances => Db.Queryable<StockBalance>();
    public ISugarQueryable<StockCount> StockCounts => Db.Queryable<StockCount>();
    public ISugarQueryable<StockCountLine> StockCountLines => Db.Queryable<StockCountLine>();
    public ISugarQueryable<PurchaseReturn> PurchaseReturns => Db.Queryable<PurchaseReturn>();
    public ISugarQueryable<PurchaseReturnLine> PurchaseReturnLines => Db.Queryable<PurchaseReturnLine>();
    public ISugarQueryable<SalesReturn> SalesReturns => Db.Queryable<SalesReturn>();
    public ISugarQueryable<SalesReturnLine> SalesReturnLines => Db.Queryable<SalesReturnLine>();
}