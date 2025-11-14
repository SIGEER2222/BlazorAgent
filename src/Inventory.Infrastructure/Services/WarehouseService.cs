using Inventory.Domain.Entities;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Services;

public class WarehouseService
{
    readonly InventoryDb _db;
    public WarehouseService(InventoryDb db) { _db = db; }

    public Task<(IEnumerable<Warehouse> items, int total)> QueryAsync(int page, int pageSize, string? keyword)
    {
        var q = _db.Warehouses;
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(p => p.Name.Contains(keyword) || p.Code.Contains(keyword));
        int total = 0;
        var list = q.OrderBy(p => p.Id).ToPageList(page, pageSize, ref total);
        return Task.FromResult(((IEnumerable<Warehouse>)list, total));
    }

    public Task<Warehouse?> GetAsync(int id) => Task.FromResult(_db.Db.Queryable<Warehouse>().InSingle(id));
    public Task<int> CreateAsync(Warehouse p) { _db.Db.Insertable(p).ExecuteCommand(); return Task.FromResult(p.Id); }
    public Task UpdateAsync(Warehouse p) { _db.Db.Updateable(p).ExecuteCommand(); return Task.CompletedTask; }
    public Task DeleteAsync(int id) { _db.Db.Deleteable<Warehouse>().In(id).ExecuteCommand(); return Task.CompletedTask; }
}