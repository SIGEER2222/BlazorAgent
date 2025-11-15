namespace Inventory.Domain.Entities;

public class Unit
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Symbol { get; set; }
    public bool Enabled { get; set; } = true;
}