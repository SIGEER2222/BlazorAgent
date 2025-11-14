using Inventory.Domain.Entities;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Services;

public class UnitService
{
    readonly InventoryDb _db;
    public UnitService(InventoryDb db) { _db = db; }

    public Task<(IEnumerable<Unit> items, int total)> QueryAsync(int page, int pageSize, string? keyword)
    {
        var q = _db.Units;
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(p => p.Name.Contains(keyword) || p.Symbol!.Contains(keyword));
        int total = 0;
        var list = q.OrderBy(p => p.Id).ToPageList(page, pageSize, ref total);
        return Task.FromResult(((IEnumerable<Unit>)list, total));
    }

    public Task<Unit?> GetAsync(int id) => Task.FromResult(_db.Db.Queryable<Unit>().InSingle(id));
    public Task<int> CreateAsync(Unit p) { _db.Db.Insertable(p).ExecuteCommand(); return Task.FromResult(p.Id); }
    public Task UpdateAsync(Unit p) { _db.Db.Updateable(p).ExecuteCommand(); return Task.CompletedTask; }
    public Task DeleteAsync(int id) { _db.Db.Deleteable<Unit>().In(id).ExecuteCommand(); return Task.CompletedTask; }
}