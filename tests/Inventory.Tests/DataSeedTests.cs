using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Services;
using Inventory.Domain.Entities;
using Xunit;
using Bogus;
using System.Linq;

public class DataSeedTests
{
    static string AppConn()
    {
        var path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "src", "Inventory.Web", "App_Data", "inventory.db");
        var dir = System.IO.Path.GetDirectoryName(path)!;
        if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
        return $"Data Source={path}";
    }

    [Fact]
    public async void GenerateDemoData()
    {
        var db = new InventoryDb(AppConn());
        var inv = new InventoryService(db);
        var audit = new AuditService(db, new TestUser("seed@test"));
        var ps = new PurchaseService(db, inv, audit);
        var ss = new SalesService(db, inv, audit);
        var ts = new TransferService(db, inv, audit);
        var st = new StocktakingService(db, inv, audit);
        var prs = new PurchaseReturnService(db, inv, audit);
        var srs = new SalesReturnService(db, inv, audit);

        var f = new Faker("zh_CN");

        if (!db.Warehouses.Any())
        {
            db.Db.Insertable(new Warehouse { Code = "WH1", Name = "主仓", Location = "" }).ExecuteCommand();
            db.Db.Insertable(new Warehouse { Code = "WH2", Name = "二号仓", Location = "" }).ExecuteCommand();
        }

        if (!db.Units.Any())
        {
            db.Db.Insertable(new Unit { Name = "件", Symbol = "pcs", Enabled = true }).ExecuteCommand();
        }

        if (db.Products.Count() < 20)
        {
            var cats = new[] { "默认", "电子", "耗材" };
            var prods = new Faker<Product>("zh_CN")
                .RuleFor(p => p.Code, f => "P" + f.Random.Number(1000, 9999))
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Category, f => f.PickRandom(cats))
                .RuleFor(p => p.Unit, _ => "件")
                .RuleFor(p => p.Enabled, _ => true)
                .Generate(30);
            db.Db.Insertable(prods).ExecuteCommand();
        }

        if (db.Suppliers.Count() < 10)
        {
            var sups = new Faker<Supplier>("zh_CN")
                .RuleFor(s => s.Code, f => "SUP" + f.Random.Number(100, 999))
                .RuleFor(s => s.Name, f => f.Company.CompanyName())
                .RuleFor(s => s.Contact, f => f.Name.FullName())
                .RuleFor(s => s.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(s => s.Email, f => f.Internet.Email())
                .RuleFor(s => s.Address, f => f.Address.FullAddress())
                .RuleFor(s => s.Enabled, _ => true)
                .Generate(12);
            db.Db.Insertable(sups).ExecuteCommand();
        }

        if (db.Customers.Count() < 10)
        {
            var custs = new Faker<Customer>("zh_CN")
                .RuleFor(s => s.Code, f => "CUST" + f.Random.Number(100, 999))
                .RuleFor(s => s.Name, f => f.Company.CompanyName())
                .RuleFor(s => s.Contact, f => f.Name.FullName())
                .RuleFor(s => s.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(s => s.Email, f => f.Internet.Email())
                .RuleFor(s => s.Address, f => f.Address.FullAddress())
                .RuleFor(s => s.Enabled, _ => true)
                .Generate(12);
            db.Db.Insertable(custs).ExecuteCommand();
        }

        var wh1 = db.Warehouses.First(x => x.Code == "WH1");
        var wh2 = db.Warehouses.First(x => x.Code == "WH2");
        var prodIds = db.Products.Select(p => p.Id).ToList();

        // 生成采购订单并收货
        for (int i = 0; i < 10; i++)
        {
            var supCodes = db.Suppliers.Select(s => s.Code).ToList();
            var supCode = supCodes[f.Random.Int(0, supCodes.Count - 1)];
            var po = new PurchaseOrder { Code = $"PO-{f.Random.Number(1000,9999)}", SupplierCode = supCode, Remark = "" };
            await ps.CreateAsync(po);
            var pickPo = prodIds.OrderBy(_ => f.Random.Number()).Take(f.Random.Int(2,4)).ToList();
            foreach (var id in pickPo)
            {
                await ps.AddLineAsync(new PurchaseOrderLine { PurchaseOrderId = po.Id, ProductId = id, WarehouseId = wh1.Id, Quantity = f.Random.Number(5, 20), UnitPrice = f.Random.Number(5, 30) });
            }
            await ps.ApproveAsync(po.Id);
            await ps.ReceiveAsync(po.Id);
        }

        // 生成销售订单并发货
        for (int i = 0; i < 8; i++)
        {
            var custCodes = db.Customers.Select(s => s.Code).ToList();
            var custCode = custCodes[f.Random.Int(0, custCodes.Count - 1)];
            var so = new SalesOrder { Code = $"SO-{f.Random.Number(1000,9999)}", CustomerCode = custCode, Remark = "" };
            await ss.CreateAsync(so);
            var pickSo = prodIds.OrderBy(_ => f.Random.Number()).Take(f.Random.Int(1,3)).ToList();
            foreach (var id in pickSo)
            {
                await ss.AddLineAsync(new SalesOrderLine { SalesOrderId = so.Id, ProductId = id, WarehouseId = wh1.Id, Quantity = f.Random.Number(1, 8), UnitPrice = f.Random.Number(10, 50) });
            }
            await ss.ApproveAsync(so.Id);
            await ss.ShipAsync(so.Id);
        }

        // 调拨与盘点
        var firstProd = prodIds[0];
        var avgCost = db.Balances.Where(b => b.WarehouseId == wh1.Id && b.ProductId == firstProd).First().AvgCost;
        await ts.TransferAsync(wh1.Id, wh2.Id, firstProd, 10m, avgCost, $"TR-{f.Random.Number(100,999)}");
        var sc = new StockCount { Code = $"ST-{f.Random.Number(1000,9999)}" }; await st.CreateAsync(sc);
        await st.AddLineAsync(new StockCountLine { StockCountId = sc.Id, WarehouseId = wh1.Id, ProductId = prodIds[prodIds.Count-1], CountedQty = 20m });
        await st.ApplyAsync(sc.Id);

        // 退货
        var pr = new PurchaseReturn { Code = $"PR-{f.Random.Number(1000,9999)}", SupplierCode = db.Suppliers.First().Code, Remark = "" };
        await prs.CreateAsync(pr);
        await prs.AddLineAsync(new PurchaseReturnLine { PurchaseReturnId = pr.Id, WarehouseId = wh1.Id, ProductId = prodIds[1], Quantity = 2m, UnitPrice = 10m });
        await prs.ProcessAsync(pr.Id);

        var sr = new SalesReturn { Code = $"SR-{f.Random.Number(1000,9999)}", CustomerCode = db.Customers.First().Code, Remark = "" };
        await srs.CreateAsync(sr);
        await srs.AddLineAsync(new SalesReturnLine { SalesReturnId = sr.Id, WarehouseId = wh1.Id, ProductId = prodIds[2], Quantity = 1m, UnitPrice = 20m });
        await srs.ProcessAsync(sr.Id);
    }

    class TestUser : Inventory.Infrastructure.Services.ICurrentUser
    {
        public TestUser(string? name) { Name = name; }
        public string? Name { get; }
    }
}