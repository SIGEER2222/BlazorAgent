using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Services;
using Inventory.Domain.Entities;
using Xunit;
using System.Linq;

public class ReturnServiceTests
{
    static string NewConn() => $"Data Source={System.IO.Path.Combine(System.IO.Path.GetTempPath(), "inventory_test_" + System.Guid.NewGuid().ToString("N") + ".db")}";

    [Fact]
    public async void PurchaseReturn_DecrementsStock()
    {
        var db = new InventoryDb(NewConn()); var inv = new InventoryService(db); var audit = new AuditService(db, new Inventory.Tests.Helpers.TestUser("test")); var svc = new PurchaseReturnService(db, inv, audit);
        var p = new Product { Code = "PR001", Name = "Prod", Category = "默认", Unit = "件" }; db.Db.Insertable(p).ExecuteCommand();
        var wh = new Warehouse { Code = "WH", Name = "Main", Location = "" }; db.Db.Insertable(wh).ExecuteCommand();
        await inv.InboundAsync(wh.Id, p.Id, 10m, 5m, "init");
        var r = new PurchaseReturn { Code = "PR-1", SupplierCode = "S001", Remark = "" }; await svc.CreateAsync(r);
        await svc.AddLineAsync(new PurchaseReturnLine { PurchaseReturnId = r.Id, ProductId = p.Id, WarehouseId = wh.Id, Quantity = 4m, UnitPrice = 5m });
        var ok = await svc.ProcessAsync(r.Id); Assert.True(ok);
        var bal = db.Balances.First(x => x.WarehouseId == wh.Id && x.ProductId == p.Id);
        Assert.Equal(6m, bal.Quantity);
    }

    [Fact]
    public async void SalesReturn_IncrementsStock()
    {
        var db = new InventoryDb(NewConn()); var inv = new InventoryService(db); var audit = new AuditService(db, new Inventory.Tests.Helpers.TestUser("test")); var svc = new SalesReturnService(db, inv, audit);
        var p = new Product { Code = "SR001", Name = "Prod", Category = "默认", Unit = "件" }; db.Db.Insertable(p).ExecuteCommand();
        var wh = new Warehouse { Code = "WH", Name = "Main", Location = "" }; db.Db.Insertable(wh).ExecuteCommand();
        await inv.InboundAsync(wh.Id, p.Id, 5m, 10m, "init");
        var r = new SalesReturn { Code = "SR-1", CustomerCode = "C001", Remark = "" }; await svc.CreateAsync(r);
        await svc.AddLineAsync(new SalesReturnLine { SalesReturnId = r.Id, ProductId = p.Id, WarehouseId = wh.Id, Quantity = 3m, UnitPrice = 10m });
        var ok = await svc.ProcessAsync(r.Id); Assert.True(ok);
        var bal = db.Balances.First(x => x.WarehouseId == wh.Id && x.ProductId == p.Id);
        Assert.Equal(8m, bal.Quantity);
    }
}