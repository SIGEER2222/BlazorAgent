using Inventory.Domain.Entities;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Services;

public class PurchaseReturnService
{
    readonly InventoryDb _db;
    readonly InventoryService _inv;
    readonly AuditService _audit;
    public PurchaseReturnService(InventoryDb db, InventoryService inv, AuditService audit) { _db = db; _inv = inv; _audit = audit; }

    public Task<(IEnumerable<PurchaseReturn> items, int total)> QueryAsync(int page, int pageSize, string? keyword)
    {
        var q = _db.PurchaseReturns;
        if (!string.IsNullOrWhiteSpace(keyword)) q = q.Where(p => p.Code.Contains(keyword) || p.SupplierCode.Contains(keyword));
        int total = 0; var list = q.OrderBy(p => p.CreatedAt, SqlSugar.OrderByType.Desc).ToPageList(page, pageSize, ref total);
        return Task.FromResult(((IEnumerable<PurchaseReturn>)list, total));
    }

    public Task<int> CreateAsync(PurchaseReturn r) { _db.Db.Insertable(r).ExecuteCommand(); return Task.FromResult(r.Id); }
    public Task AddLineAsync(PurchaseReturnLine line) { _db.Db.Insertable(line).ExecuteCommand(); return Task.CompletedTask; }
    public Task<List<PurchaseReturnLine>> GetLinesAsync(int id) => Task.FromResult(_db.Db.Queryable<PurchaseReturnLine>().Where(x => x.PurchaseReturnId == id).ToList());

    public async Task<bool> ProcessAsync(int id)
    {
        var r = _db.Db.Queryable<PurchaseReturn>().First(x => x.Id == id); if (r == null || r.Status != ReturnStatus.Draft) return false;
        var lines = await GetLinesAsync(id);
        if (lines.Count == 0) return false;
        try
        {
            _db.Db.Ado.BeginTran();
            foreach (var l in lines)
            {
                var ok = await _inv.OutboundAsync(l.WarehouseId, l.ProductId, l.Quantity, l.UnitPrice, $"PR#{r.Code}");
                if (!ok) throw new InvalidOperationException("库存不足，无法退货");
            }
            _db.Db.Updateable<PurchaseReturn>().SetColumns(p => new PurchaseReturn { Status = ReturnStatus.Processed }).Where(p => p.Id == id).ExecuteCommand();
            _db.Db.Ado.CommitTran();
            await _audit.LogAsync("Purchase.Return", nameof(PurchaseReturn), r.Code, lines);
            return true;
        }
        catch
        {
            _db.Db.Ado.RollbackTran();
            return false;
        }
    }

    public Task<bool> DeleteAsync(int id)
    {
        var r = _db.Db.Queryable<PurchaseReturn>().First(x => x.Id == id);
        if (r == null || r.Status != ReturnStatus.Draft) return Task.FromResult(false);
        _db.Db.Deleteable<PurchaseReturnLine>().Where(x => x.PurchaseReturnId == id).ExecuteCommand();
        _db.Db.Deleteable<PurchaseReturn>().Where(x => x.Id == id).ExecuteCommand();
        return Task.FromResult(true);
    }
}