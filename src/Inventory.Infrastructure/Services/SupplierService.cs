using Inventory.Domain.Entities;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Services;

public class SupplierService
{
    readonly InventoryDb _db;
    public SupplierService(InventoryDb db) { _db = db; }

    public Task<(IEnumerable<Supplier> items, int total)> QueryAsync(int page, int pageSize, string? keyword)
    {
        var q = _db.Suppliers;
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(p => p.Name.Contains(keyword) || p.Code.Contains(keyword));
        int total = 0;
        var list = q.OrderBy(p => p.Id).ToPageList(page, pageSize, ref total);
        return Task.FromResult(((IEnumerable<Supplier>)list, total));
    }

    public Task<Supplier?> GetAsync(int id) => Task.FromResult(_db.Db.Queryable<Supplier>().InSingle(id));
    public Task<int> CreateAsync(Supplier p) { _db.Db.Insertable(p).ExecuteCommand(); return Task.FromResult(p.Id); }
    public Task UpdateAsync(Supplier p) { _db.Db.Updateable(p).ExecuteCommand(); return Task.CompletedTask; }
    public Task DeleteAsync(int id) { _db.Db.Deleteable<Supplier>().In(id).ExecuteCommand(); return Task.CompletedTask; }
}