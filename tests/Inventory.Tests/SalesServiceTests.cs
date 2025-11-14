using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Services;
using Inventory.Domain.Entities;
using Xunit;
using System.Linq;

public class SalesServiceTests
{
    static string NewConn() => $"Data Source={System.IO.Path.Combine(System.IO.Path.GetTempPath(), "inventory_test_" + System.Guid.NewGuid().ToString("N") + ".db")}";

    [Fact]
    public async void Ship_DecrementsStock_WhenEnough()
    {
        var db = new InventoryDb(NewConn());
        var inv = new InventoryService(db);
        var audit = new AuditService(db, new Inventory.Tests.Helpers.TestUser("test"));
        var sales = new SalesService(db, inv, audit);

        var p = new Product { Code = "S001", Name = "Prod", Category = "默认", Unit = "件" };
        db.Db.Insertable(p).ExecuteCommand();
        var wh = new Warehouse { Code = "WH", Name = "Main", Location = "" };
        db.Db.Insertable(wh).ExecuteCommand();

        await inv.InboundAsync(wh.Id, p.Id, 10m, 5m, "init");

        var so = new SalesOrder { Code = "SO-1", CustomerCode = "C001", Remark = "" };
        await sales.CreateAsync(so);
        await sales.AddLineAsync(new SalesOrderLine { SalesOrderId = so.Id, ProductId = p.Id, WarehouseId = wh.Id, Quantity = 7m, UnitPrice = 6m });

        var ok = await sales.ShipAsync(so.Id);
        Assert.True(ok);
        var bal = db.Balances.First(x => x.WarehouseId == wh.Id && x.ProductId == p.Id);
        Assert.Equal(3m, bal.Quantity);
        Assert.Equal(5m, bal.AvgCost);
    }

    [Fact]
    public async void Ship_Fails_WhenInsufficient()
    {
        var db = new InventoryDb(NewConn());
        var inv = new InventoryService(db);
        var audit = new AuditService(db, new Inventory.Tests.Helpers.TestUser("test"));
        var sales = new SalesService(db, inv, audit);

        var p = new Product { Code = "S002", Name = "Prod2", Category = "默认", Unit = "件" };
        db.Db.Insertable(p).ExecuteCommand();
        var wh = new Warehouse { Code = "WH2", Name = "Main2", Location = "" };
        db.Db.Insertable(wh).ExecuteCommand();

        await inv.InboundAsync(wh.Id, p.Id, 2m, 10m, "init");

        var so = new SalesOrder { Code = "SO-2", CustomerCode = "C001", Remark = "" };
        await sales.CreateAsync(so);
        await sales.AddLineAsync(new SalesOrderLine { SalesOrderId = so.Id, ProductId = p.Id, WarehouseId = wh.Id, Quantity = 5m, UnitPrice = 11m });

        var ok = await sales.ShipAsync(so.Id);
        Assert.False(ok);
        var bal = db.Balances.First(x => x.WarehouseId == wh.Id && x.ProductId == p.Id);
        Assert.Equal(2m, bal.Quantity);
    }
}