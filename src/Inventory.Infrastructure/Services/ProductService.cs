using Inventory.Domain.Entities;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Services;

public class ProductService
{
    readonly InventoryDb _db;
    public ProductService(InventoryDb db) { _db = db; }

    public Task<(IEnumerable<Product> items, int total)> QueryAsync(int page, int pageSize, string? keyword)
    {
        var q = _db.Products;
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(p => p.Name.Contains(keyword) || p.Code.Contains(keyword));
        int total = 0;
        var list = q.OrderBy(p => p.Id).ToPageList(page, pageSize, ref total);
        return Task.FromResult(((IEnumerable<Product>)list, total));
    }

    public Task<Product?> GetAsync(int id) => Task.FromResult(_db.Db.Queryable<Product>().InSingle(id));

    public Task<Product?> FindByCodeAsync(string code)
    {
        var p = _db.Db.Queryable<Product>().First(x => x.Code == code);
        return Task.FromResult(p);
    }

    public Task<int> CreateAsync(Product p)
    {
        _db.Db.Insertable(p).ExecuteCommand();
        return Task.FromResult(p.Id);
    }

    public Task UpdateAsync(Product p)
    {
        _db.Db.Updateable(p).ExecuteCommand();
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        _db.Db.Deleteable<Product>().In(id).ExecuteCommand();
        return Task.CompletedTask;
    }
}