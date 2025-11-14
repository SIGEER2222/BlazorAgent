using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Services;
using Inventory.Domain.Entities;
using Xunit;
using System.Linq;

public class PurchaseServiceTests
{
    static string NewConn() => $"Data Source={System.IO.Path.Combine(System.IO.Path.GetTempPath(), "inventory_test_" + System.Guid.NewGuid().ToString("N") + ".db")}";

    [Fact]
    public async void Receive_IncrementsStock()
    {
        var db = new InventoryDb(NewConn());
        var inv = new InventoryService(db);
        var audit = new AuditService(db, new Inventory.Tests.Helpers.TestUser("test"));
        var svc = new PurchaseService(db, inv, audit);

        var p = new Product { Code = "P001", Name = "Prod", Category = "默认", Unit = "件" };
        db.Db.Insertable(p).ExecuteCommand();
        var wh = new Warehouse { Code = "WH", Name = "Main", Location = "" };
        db.Db.Insertable(wh).ExecuteCommand();

        var po = new PurchaseOrder { Code = "PO-1", SupplierCode = "S001", Remark = "" };
        await svc.CreateAsync(po);
        await svc.AddLineAsync(new PurchaseOrderLine { PurchaseOrderId = po.Id, ProductId = p.Id, WarehouseId = wh.Id, Quantity = 8m, UnitPrice = 12m });

        await svc.ReceiveAsync(po.Id);
        var bal = db.Balances.First(x => x.WarehouseId == wh.Id && x.ProductId == p.Id);
        Assert.Equal(8m, bal.Quantity);
        Assert.Equal(12m, bal.AvgCost);
    }
}