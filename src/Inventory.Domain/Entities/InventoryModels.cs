namespace Inventory.Domain.Entities;

public enum MovementType { Inbound = 1, Outbound = 2, TransferOut = 3, TransferIn = 4, AdjustIncrease = 5, AdjustDecrease = 6 }

public class InventoryMovement
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public MovementType Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string? Reference { get; set; }
}

public class StockBalance
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal AvgCost { get; set; }
}