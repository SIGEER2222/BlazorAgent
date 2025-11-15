using Inventory.Domain.Entities;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Services;

public class CategoryService
{
    readonly InventoryDb _db;
    public CategoryService(InventoryDb db) { _db = db; }

    public Task<(IEnumerable<Category> items, int total)> QueryAsync(int page, int pageSize, string? keyword)
    {
        var q = _db.Categories;
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(p => p.Name.Contains(keyword));
        int total = 0;
        var list = q.OrderBy(p => p.Id).ToPageList(page, pageSize, ref total);
        return Task.FromResult(((IEnumerable<Category>)list, total));
    }

    public Task<Category?> GetAsync(int id) => Task.FromResult(_db.Db.Queryable<Category>().InSingle(id));
    public Task<int> CreateAsync(Category p) { _db.Db.Insertable(p).ExecuteCommand(); return Task.FromResult(p.Id); }
    public Task UpdateAsync(Category p) { _db.Db.Updateable(p).ExecuteCommand(); return Task.CompletedTask; }
    public Task DeleteAsync(int id) { _db.Db.Deleteable<Category>().In(id).ExecuteCommand(); return Task.CompletedTask; }
}