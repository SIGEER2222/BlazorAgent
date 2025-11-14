namespace Inventory.Domain.Entities;

public enum StockCountStatus { Draft = 0, Applied = 2, Canceled = 9 }

public class StockCount
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public StockCountStatus Status { get; set; } = StockCountStatus.Draft;
}

public class StockCountLine
{
    public int Id { get; set; }
    public int StockCountId { get; set; }
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public decimal CountedQty { get; set; }
}