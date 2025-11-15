namespace Inventory.Web.Data;

public static class SeedData
{
  public static void EnsureDemo(Inventory.Infrastructure.Data.InventoryDb db)
  {
    var wh1 = db.Warehouses.First(x => x.Code == "WH1");
    if (wh1 == null)
    {
      db.Db.Insertable(new Inventory.Domain.Entities.Warehouse { Code = "WH1", Name = "主仓", Location = "总部" }).ExecuteCommand();
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

    EnsureBalance("P002", 100, 10);
    EnsureBalance("P003", 50, 8);
  }
}