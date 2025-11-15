namespace Inventory.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public DateTime Time { get; set; } = DateTime.UtcNow;
    public string? User { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? DataJson { get; set; }
}
