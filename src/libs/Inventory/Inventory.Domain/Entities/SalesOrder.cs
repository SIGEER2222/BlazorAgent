namespace Inventory.Domain.Entities;

public enum SalesOrderStatus { Draft = 0, Approved = 1, Shipped = 2, Canceled = 9 }

public class SalesOrder
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
    public string? Remark { get; set; }
}

public class SalesOrderLine
{
    public int Id { get; set; }
    public int SalesOrderId { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}