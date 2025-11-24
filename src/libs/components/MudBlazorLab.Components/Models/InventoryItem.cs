namespace MudBlazorLab.Components.Models;

public enum InventoryCategory { General, Perishable, Valuable, Hazardous }

public class InventoryItem
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public double WeightKg { get; set; }
    public bool Active { get; set; }
    public DateTime ManufactureDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public InventoryCategory Category { get; set; } = InventoryCategory.General;
}