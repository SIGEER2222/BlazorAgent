using Inventory.Domain.Entities;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Services;

public class CustomerService
{
    readonly InventoryDb _db;
    public CustomerService(InventoryDb db) { _db = db; }

    public Task<(IEnumerable<Customer> items, int total)> QueryAsync(int page, int pageSize, string? keyword)
    {
        var q = _db.Customers;
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(p => p.Name.Contains(keyword) || p.Code.Contains(keyword));
        int total = 0;
        var list = q.OrderBy(p => p.Id).ToPageList(page, pageSize, ref total);
        return Task.FromResult(((IEnumerable<Customer>)list, total));
    }

    public Task<Customer?> GetAsync(int id) => Task.FromResult(_db.Db.Queryable<Customer>().InSingle(id));
    public Task<int> CreateAsync(Customer p) { _db.Db.Insertable(p).ExecuteCommand(); return Task.FromResult(p.Id); }
    public Task UpdateAsync(Customer p) { _db.Db.Updateable(p).ExecuteCommand(); return Task.CompletedTask; }
    public Task DeleteAsync(int id) { _db.Db.Deleteable<Customer>().In(id).ExecuteCommand(); return Task.CompletedTask; }
}