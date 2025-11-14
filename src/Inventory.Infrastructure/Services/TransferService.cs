using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Services;

public class TransferService
{
    readonly InventoryDb _db;
    readonly InventoryService _inv;
    readonly AuditService _audit;
    public TransferService(InventoryDb db, InventoryService inv, AuditService audit) { _db = db; _inv = inv; _audit = audit; }

    public async Task<bool> TransferAsync(int fromWarehouseId, int toWarehouseId, int productId, decimal qty, decimal unitCost, string reference)
    {
        var ok = await _inv.OutboundAsync(fromWarehouseId, productId, qty, unitCost, reference);
        if (!ok) return false;
        await _inv.InboundAsync(toWarehouseId, productId, qty, unitCost, reference);
        await _audit.LogAsync("Inventory.Transfer", "Inventory", reference, new { fromWarehouseId, toWarehouseId, productId, qty, unitCost });
        return true;
    }
}