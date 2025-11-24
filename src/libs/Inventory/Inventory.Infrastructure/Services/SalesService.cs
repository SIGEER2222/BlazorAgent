using Inventory.Domain.Entities;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Services;

public class SalesService
{
    readonly InventoryDb _db;
    readonly InventoryService _inv;
    readonly AuditService _audit;
    public SalesService(InventoryDb db, InventoryService inv, AuditService audit) { _db = db; _inv = inv; _audit = audit; }

    public Task<(IEnumerable<SalesOrder> items, int total)> QueryAsync(int page, int pageSize, string? keyword)
    {
        var q = _db.SalesOrders;
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(p => p.Code.Contains(keyword) || p.CustomerCode.Contains(keyword));
        int total = 0;
        var list = q.OrderBy(p => p.CreatedAt, SqlSugar.OrderByType.Desc).ToPageList(page, pageSize, ref total);
        return Task.FromResult(((IEnumerable<SalesOrder>)list, total));
    }

    public Task<int> CreateAsync(SalesOrder so)
    {
        if (string.IsNullOrWhiteSpace(so.Code))
            so.Code = $"SO-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        so.Remark ??= string.Empty;
        so.Status = SalesOrderStatus.Draft;
        _db.Db.Insertable(so).ExecuteCommand();
        return Task.FromResult(so.Id);
    }

    public Task AddLineAsync(SalesOrderLine line)
    {
        _db.Db.Insertable(line).ExecuteCommand();
        return Task.CompletedTask;
    }

    public Task<List<SalesOrderLine>> GetLinesAsync(int soId)
        => Task.FromResult(_db.Db.Queryable<SalesOrderLine>().Where(x => x.SalesOrderId == soId).ToList());

    public Task ApproveAsync(int soId)
    {
        var so = _db.Db.Queryable<SalesOrder>().First(x => x.Id == soId);
        if (so == null || so.Status != SalesOrderStatus.Draft) return Task.CompletedTask;
        var lines = _db.Db.Queryable<SalesOrderLine>().Where(x => x.SalesOrderId == soId).Any();
        if (!lines) return Task.CompletedTask;
        _db.Db.Updateable<SalesOrder>().SetColumns(p => new SalesOrder { Status = SalesOrderStatus.Approved }).Where(p => p.Id == soId).ExecuteCommand();
        return Task.CompletedTask;
    }

    public async Task<bool> ShipAsync(int soId)
    {
        var so = _db.Db.Queryable<SalesOrder>().First(x => x.Id == soId);
        if (so == null || so.Status != SalesOrderStatus.Approved) return false;
        var lines = await GetLinesAsync(soId);
        try
        {
            _db.Db.Ado.BeginTran();
            foreach (var l in lines)
            {
                var ok = await _inv.OutboundAsync(l.WarehouseId, l.ProductId, l.Quantity, l.UnitPrice, $"SO#{so.Code}");
                if (!ok) throw new InvalidOperationException("库存不足，无法发货");
            }
            _db.Db.Updateable<SalesOrder>().SetColumns(p => new SalesOrder { Status = SalesOrderStatus.Shipped }).Where(p => p.Id == soId).ExecuteCommand();
            _db.Db.Ado.CommitTran();
            await _audit.LogAsync("Sales.Ship", nameof(SalesOrder), so.Code, lines);
            return true;
        }
        catch
        {
            _db.Db.Ado.RollbackTran();
            return false;
        }
    }

    public Task<bool> DeleteAsync(int soId)
    {
        var so = _db.Db.Queryable<SalesOrder>().First(x => x.Id == soId);
        if (so == null || so.Status != SalesOrderStatus.Draft) return Task.FromResult(false);
        _db.Db.Deleteable<SalesOrderLine>().Where(x => x.SalesOrderId == soId).ExecuteCommand();
        _db.Db.Deleteable<SalesOrder>().Where(x => x.Id == soId).ExecuteCommand();
        return Task.FromResult(true);
    }
}