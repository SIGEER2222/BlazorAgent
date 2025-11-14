using Inventory.Domain.Entities;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Services;

public class InventoryService
{
    readonly InventoryDb _db;
    public InventoryService(InventoryDb db) { _db = db; }

    public Task InboundAsync(int warehouseId, int productId, decimal qty, decimal unitCost, string reference)
    {
        _db.Db.Insertable(new InventoryMovement
        {
            WarehouseId = warehouseId,
            ProductId = productId,
            Type = MovementType.Inbound,
            Quantity = qty,
            UnitCost = unitCost,
            Reference = reference,
        }).ExecuteCommand();

        var bal = _db.Db.Queryable<StockBalance>().First(x => x.WarehouseId == warehouseId && x.ProductId == productId);
        if (bal == null)
        {
            var newBal = new StockBalance { WarehouseId = warehouseId, ProductId = productId, Quantity = qty, AvgCost = unitCost };
            _db.Db.Insertable(newBal).ExecuteCommand();
        }
        else
        {
            var newQty = bal.Quantity + qty;
            var newAvg = newQty == 0 ? 0 : ((bal.Quantity * bal.AvgCost) + (qty * unitCost)) / newQty;
            _db.Db.Updateable<StockBalance>()
                .SetColumns(x => new StockBalance { Quantity = newQty, AvgCost = newAvg })
                .Where(x => x.WarehouseId == warehouseId && x.ProductId == productId)
                .ExecuteCommand();
        }

        return Task.CompletedTask;
    }

    public Task<bool> OutboundAsync(int warehouseId, int productId, decimal qty, decimal unitCost, string reference)
    {
        var bal = _db.Db.Queryable<StockBalance>().First(x => x.WarehouseId == warehouseId && x.ProductId == productId);
        if (bal == null || bal.Quantity < qty)
        {
            return Task.FromResult(false);
        }
        _db.Db.Insertable(new InventoryMovement
        {
            WarehouseId = warehouseId,
            ProductId = productId,
            Type = MovementType.Outbound,
            Quantity = qty,
            UnitCost = unitCost,
            Reference = reference,
        }).ExecuteCommand();

        var newQty = bal.Quantity - qty;
        _db.Db.Updateable<StockBalance>()
            .SetColumns(x => new StockBalance { Quantity = newQty })
            .Where(x => x.WarehouseId == warehouseId && x.ProductId == productId)
            .ExecuteCommand();
        return Task.FromResult(true);
    }
}