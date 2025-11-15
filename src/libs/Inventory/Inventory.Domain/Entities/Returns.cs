namespace Inventory.Domain.Entities;

public enum ReturnStatus { Draft = 0, Processed = 2, Canceled = 9 }

public class PurchaseReturn
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ReturnStatus Status { get; set; } = ReturnStatus.Draft;
    public string? Remark { get; set; }
}

public class PurchaseReturnLine
{
    public int Id { get; set; }
    public int PurchaseReturnId { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class SalesReturn
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ReturnStatus Status { get; set; } = ReturnStatus.Draft;
    public string? Remark { get; set; }
}

public class SalesReturnLine
{
    public int Id { get; set; }
    public int SalesReturnId { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}