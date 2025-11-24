using Inventory.Domain.Entities;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Services;

public class PurchaseService
{
    readonly InventoryDb _db;
    readonly InventoryService _inv;
    readonly AuditService _audit;
    public PurchaseService(InventoryDb db, InventoryService inv, AuditService audit) { _db = db; _inv = inv; _audit = audit; }

    public Task<(IEnumerable<PurchaseOrder> items, int total)> QueryAsync(int page, int pageSize, string? keyword)
    {
        var q = _db.PurchaseOrders;
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(p => p.Code.Contains(keyword) || p.SupplierCode.Contains(keyword));
        int total = 0;
        var list = q.OrderBy(p => p.CreatedAt, SqlSugar.OrderByType.Desc).ToPageList(page, pageSize, ref total);
        return Task.FromResult(((IEnumerable<PurchaseOrder>)list, total));
    }

    public Task<int> CreateAsync(PurchaseOrder po)
    {
        if (string.IsNullOrWhiteSpace(po.Code))
            po.Code = $"PO-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        po.Remark ??= string.Empty;
        po.Status = PurchaseOrderStatus.Draft;
        _db.Db.Insertable(po).ExecuteCommand();
        return Task.FromResult(po.Id);
    }

    public Task AddLineAsync(PurchaseOrderLine line)
    {
        _db.Db.Insertable(line).ExecuteCommand();
        return Task.CompletedTask;
    }

    public Task<List<PurchaseOrderLine>> GetLinesAsync(int poId)
        => Task.FromResult(_db.Db.Queryable<PurchaseOrderLine>().Where(x => x.PurchaseOrderId == poId).ToList());

    public Task ApproveAsync(int poId)
    {
        var po = _db.Db.Queryable<PurchaseOrder>().First(x => x.Id == poId);
        if (po == null || po.Status != PurchaseOrderStatus.Draft) return Task.CompletedTask;
        var lines = _db.Db.Queryable<PurchaseOrderLine>().Where(x => x.PurchaseOrderId == poId).Any();
        if (!lines) return Task.CompletedTask;
        _db.Db.Updateable<PurchaseOrder>().SetColumns(p => new PurchaseOrder { Status = PurchaseOrderStatus.Approved }).Where(p => p.Id == poId).ExecuteCommand();
        return Task.CompletedTask;
    }

    public async Task ReceiveAsync(int poId)
    {
        var po = _db.Db.Queryable<PurchaseOrder>().First(x => x.Id == poId);
        if (po == null || po.Status != PurchaseOrderStatus.Approved) return;
        var lines = await GetLinesAsync(poId);
        try
        {
            _db.Db.Ado.BeginTran();
            foreach (var l in lines)
            {
                await _inv.InboundAsync(l.WarehouseId, l.ProductId, l.Quantity, l.UnitPrice, $"PO#{po.Code}");
            }
            _db.Db.Updateable<PurchaseOrder>().SetColumns(p => new PurchaseOrder { Status = PurchaseOrderStatus.Received }).Where(p => p.Id == poId).ExecuteCommand();
            _db.Db.Ado.CommitTran();
            await _audit.LogAsync("Purchase.Receive", nameof(PurchaseOrder), po.Code, lines);
        }
        catch
        {
            _db.Db.Ado.RollbackTran();
            throw;
        }
    }

    public Task<bool> DeleteAsync(int poId)
    {
        var po = _db.Db.Queryable<PurchaseOrder>().First(x => x.Id == poId);
        if (po == null || po.Status != PurchaseOrderStatus.Draft) return Task.FromResult(false);
        _db.Db.Deleteable<PurchaseOrderLine>().Where(x => x.PurchaseOrderId == poId).ExecuteCommand();
        _db.Db.Deleteable<PurchaseOrder>().Where(x => x.Id == poId).ExecuteCommand();
        return Task.FromResult(true);
    }
}