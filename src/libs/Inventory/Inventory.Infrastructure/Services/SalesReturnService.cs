using Inventory.Domain.Entities;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Services;

public class SalesReturnService
{
    readonly InventoryDb _db;
    readonly InventoryService _inv;
    readonly AuditService _audit;
    public SalesReturnService(InventoryDb db, InventoryService inv, AuditService audit) { _db = db; _inv = inv; _audit = audit; }

    public Task<(IEnumerable<SalesReturn> items, int total)> QueryAsync(int page, int pageSize, string? keyword)
    {
        var q = _db.SalesReturns;
        if (!string.IsNullOrWhiteSpace(keyword)) q = q.Where(p => p.Code.Contains(keyword) || p.CustomerCode.Contains(keyword));
        int total = 0; var list = q.OrderBy(p => p.Id).ToPageList(page, pageSize, ref total);
        return Task.FromResult(((IEnumerable<SalesReturn>)list, total));
    }

    public Task<int> CreateAsync(SalesReturn r) { _db.Db.Insertable(r).ExecuteCommand(); return Task.FromResult(r.Id); }
    public Task AddLineAsync(SalesReturnLine line) { _db.Db.Insertable(line).ExecuteCommand(); return Task.CompletedTask; }
    public Task<List<SalesReturnLine>> GetLinesAsync(int id) => Task.FromResult(_db.Db.Queryable<SalesReturnLine>().Where(x => x.SalesReturnId == id).ToList());

    public async Task<bool> ProcessAsync(int id)
    {
        var r = _db.Db.Queryable<SalesReturn>().First(x => x.Id == id); if (r == null || r.Status != ReturnStatus.Draft) return false;
        var lines = await GetLinesAsync(id);
        if (lines.Count == 0) return false;
        try
        {
            _db.Db.Ado.BeginTran();
            foreach (var l in lines)
            {
                await _inv.InboundAsync(l.WarehouseId, l.ProductId, l.Quantity, l.UnitPrice, $"SR#{r.Code}");
            }
            _db.Db.Updateable<SalesReturn>().SetColumns(p => new SalesReturn { Status = ReturnStatus.Processed }).Where(p => p.Id == id).ExecuteCommand();
            _db.Db.Ado.CommitTran();
            await _audit.LogAsync("Sales.Return", nameof(SalesReturn), r.Code, lines);
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
        var r = _db.Db.Queryable<SalesReturn>().First(x => x.Id == id);
        if (r == null || r.Status != ReturnStatus.Draft) return Task.FromResult(false);
        _db.Db.Deleteable<SalesReturnLine>().Where(x => x.SalesReturnId == id).ExecuteCommand();
        _db.Db.Deleteable<SalesReturn>().Where(x => x.Id == id).ExecuteCommand();
        return Task.FromResult(true);
    }
}