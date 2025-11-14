using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Services;
using Inventory.Domain.Entities;
using Xunit;

public class DeleteTests
{
    static string NewConn() => $"Data Source={System.IO.Path.Combine(System.IO.Path.GetTempPath(), "inventory_test_" + System.Guid.NewGuid().ToString("N") + ".db")}";

    [Fact]
    public async void DeleteDraftPurchaseOrder()
    {
        var db = new InventoryDb(NewConn()); var inv = new InventoryService(db); var audit = new AuditService(db, new Inventory.Tests.Helpers.TestUser("test")); var svc = new PurchaseService(db, inv, audit);
        var p = new Product { Code = "DP001", Name = "Prod", Category = "默认", Unit = "件" }; db.Db.Insertable(p).ExecuteCommand();
        var wh = new Warehouse { Code = "WH", Name = "Main", Location = "" }; db.Db.Insertable(wh).ExecuteCommand();
        var po = new PurchaseOrder { Code = "PO-X", SupplierCode = "S001", Remark = "" }; await svc.CreateAsync(po);
        await svc.AddLineAsync(new PurchaseOrderLine { PurchaseOrderId = po.Id, ProductId = p.Id, WarehouseId = wh.Id, Quantity = 2m, UnitPrice = 10m });
        var ok = await svc.DeleteAsync(po.Id);
        Assert.True(ok);
        Assert.False(db.PurchaseOrders.Any(x => x.Id == po.Id));
        Assert.False(db.PurchaseOrderLines.Any(x => x.PurchaseOrderId == po.Id));
    }

    [Fact]
    public async void DeleteDraftSalesOrder()
    {
        var db = new InventoryDb(NewConn()); var inv = new InventoryService(db); var audit = new AuditService(db, new Inventory.Tests.Helpers.TestUser("test")); var svc = new SalesService(db, inv, audit);
        var p = new Product { Code = "DS001", Name = "Prod", Category = "默认", Unit = "件" }; db.Db.Insertable(p).ExecuteCommand();
        var wh = new Warehouse { Code = "WH", Name = "Main", Location = "" }; db.Db.Insertable(wh).ExecuteCommand();
        var so = new SalesOrder { Code = "SO-X", CustomerCode = "C001", Remark = "" }; await svc.CreateAsync(so);
        await svc.AddLineAsync(new SalesOrderLine { SalesOrderId = so.Id, ProductId = p.Id, WarehouseId = wh.Id, Quantity = 2m, UnitPrice = 10m });
        var ok = await svc.DeleteAsync(so.Id);
        Assert.True(ok);
        Assert.False(db.SalesOrders.Any(x => x.Id == so.Id));
        Assert.False(db.SalesOrderLines.Any(x => x.SalesOrderId == so.Id));
    }

    [Fact]
    public async void DeleteDraftReturns()
    {
        var db = new InventoryDb(NewConn()); var inv = new InventoryService(db); var audit = new AuditService(db, new Inventory.Tests.Helpers.TestUser("test"));
        var prs = new PurchaseReturnService(db, inv, audit);
        var srs = new SalesReturnService(db, inv, audit);
        var p = new Product { Code = "RT001", Name = "Prod", Category = "默认", Unit = "件" }; db.Db.Insertable(p).ExecuteCommand();
        var wh = new Warehouse { Code = "WH", Name = "Main", Location = "" }; db.Db.Insertable(wh).ExecuteCommand();
        var pr = new PurchaseReturn { Code = "PR-X", SupplierCode = "S001", Remark = "" }; await prs.CreateAsync(pr);
        await prs.AddLineAsync(new PurchaseReturnLine { PurchaseReturnId = pr.Id, ProductId = p.Id, WarehouseId = wh.Id, Quantity = 1m, UnitPrice = 5m });
        var sr = new SalesReturn { Code = "SR-X", CustomerCode = "C001", Remark = "" }; await srs.CreateAsync(sr);
        await srs.AddLineAsync(new SalesReturnLine { SalesReturnId = sr.Id, ProductId = p.Id, WarehouseId = wh.Id, Quantity = 1m, UnitPrice = 5m });
        var ok1 = await prs.DeleteAsync(pr.Id);
        var ok2 = await srs.DeleteAsync(sr.Id);
        Assert.True(ok1);
        Assert.True(ok2);
        Assert.False(db.PurchaseReturns.Any(x => x.Id == pr.Id));
        Assert.False(db.SalesReturns.Any(x => x.Id == sr.Id));
    }
}