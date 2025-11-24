using Inventory.Domain.Entities;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Services;

public class StocktakingService
{
    readonly InventoryDb _db;
    readonly InventoryService _inv;
    readonly AuditService _audit;
    public StocktakingService(InventoryDb db, InventoryService inv, AuditService audit) { _db = db; _inv = inv; _audit = audit; }

    public Task<(IEnumerable<StockCount> items, int total)> QueryAsync(int page, int pageSize, string? keyword)
    {
        var q = _db.StockCounts;
        if (!string.IsNullOrWhiteSpace(keyword)) q = q.Where(p => p.Code.Contains(keyword));
        int total = 0;
        var list = q.OrderBy(p => p.CreatedAt, SqlSugar.OrderByType.Desc).ToPageList(page, pageSize, ref total);
        return Task.FromResult(((IEnumerable<StockCount>)list, total));
    }

    public Task<int> CreateAsync(StockCount sc)
    {
        _db.Db.Insertable(sc).ExecuteCommand();
        return Task.FromResult(sc.Id);
    }

    public Task AddLineAsync(StockCountLine line)
    {
        _db.Db.Insertable(line).ExecuteCommand();
        return Task.CompletedTask;
    }

    public Task<List<StockCountLine>> GetLinesAsync(int scId)
        => Task.FromResult(_db.Db.Queryable<StockCountLine>().Where(x => x.StockCountId == scId).ToList());

    public async Task ApplyAsync(int scId)
    {
        var sc = _db.Db.Queryable<StockCount>().First(x => x.Id == scId);
        if (sc == null || sc.Status != StockCountStatus.Draft) return;
        var lines = await GetLinesAsync(scId);
        if (lines.Count == 0) return;
        foreach (var l in lines)
        {
            var bal = _db.Db.Queryable<StockBalance>().First(x => x.WarehouseId == l.WarehouseId && x.ProductId == l.ProductId) ?? new StockBalance { WarehouseId = l.WarehouseId, ProductId = l.ProductId, Quantity = 0, AvgCost = 0 };
            var diff = l.CountedQty - bal.Quantity;
            if (diff == 0) continue;
            if (diff > 0)
            {
                await _inv.InboundAsync(l.WarehouseId, l.ProductId, diff, bal.AvgCost, $"STOCKTAKE+ #{sc.Code}");
            }
            else
            {
                var ok = await _inv.OutboundAsync(l.WarehouseId, l.ProductId, Math.Abs(diff), bal.AvgCost, $"STOCKTAKE- #{sc.Code}");
                if (!ok) continue;
            }
        }
        _db.Db.Updateable<StockCount>().SetColumns(p => new StockCount { Status = StockCountStatus.Applied }).Where(p => p.Id == scId).ExecuteCommand();
        await _audit.LogAsync("Inventory.Stocktake", nameof(StockCount), sc.Code, lines);
    }
}