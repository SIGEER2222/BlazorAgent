namespace Inventory.Domain.Entities;

public enum PurchaseOrderStatus { Draft = 0, Approved = 1, Received = 2, Canceled = 9 }

public class PurchaseOrder
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public string? Remark { get; set; }
}

public class PurchaseOrderLine
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}