using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Services;
using Inventory.Domain.Entities;
using Xunit;
using System.Linq;

public class InventoryServiceTests
{
    static string NewConn() => $"Data Source={System.IO.Path.Combine(System.IO.Path.GetTempPath(), "inventory_test_" + System.Guid.NewGuid().ToString("N") + ".db")}";

    [Fact]
    public async void Inbound_UpdatesBalanceAndAverage()
    {
        var db = new InventoryDb(NewConn());
        var inv = new InventoryService(db);

        var p = new Product { Code = "T001", Name = "Test", Category = "默认", Unit = "件" };
        db.Db.Insertable(p).ExecuteCommand();
        var wh = new Warehouse { Code = "WH", Name = "Test", Location = "" };
        db.Db.Insertable(wh).ExecuteCommand();

        await inv.InboundAsync(wh.Id, p.Id, 10m, 5m, "init"); // qty=10, avg=5
        await inv.InboundAsync(wh.Id, p.Id, 10m, 15m, "add"); // qty=20, avg=(10*5+10*15)/20=10

        var bal = db.Balances.First(x => x.WarehouseId == wh.Id && x.ProductId == p.Id);
        Assert.Equal(20m, bal.Quantity);
        Assert.Equal(10m, decimal.Round(bal.AvgCost, 2));
    }

    [Fact]
    public async void Outbound_PreventsNegative()
    {
        var db = new InventoryDb(NewConn());
        var inv = new InventoryService(db);

        var p = new Product { Code = "T002", Name = "Test2", Category = "默认", Unit = "件" };
        db.Db.Insertable(p).ExecuteCommand();
        var wh = new Warehouse { Code = "WH2", Name = "Test2", Location = "" };
        db.Db.Insertable(wh).ExecuteCommand();

        await inv.InboundAsync(wh.Id, p.Id, 5m, 10m, "init");
        var ok = await inv.OutboundAsync(wh.Id, p.Id, 10m, 10m, "over");
        Assert.False(ok);

        var bal = db.Balances.First(x => x.WarehouseId == wh.Id && x.ProductId == p.Id);
        Assert.Equal(5m, bal.Quantity);
    }
}