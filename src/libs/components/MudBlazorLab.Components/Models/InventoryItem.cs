namespace MudBlazorLab.Components.Models;

public class InventoryItem
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public double WeightKg { get; set; }
    public bool Active { get; set; }
    public DateTime ManufactureDate { get; set; } = DateTime.Today;
    public DateTime? ExpiryDate { get; set; }
    public InventoryCategory Category { get; set; } = InventoryCategory.General;
}

public enum InventoryCategory
{
    [System.ComponentModel.DataAnnotations.Display(Name = "通用")]
    General = 0,
    [System.ComponentModel.DataAnnotations.Display(Name = "易腐")]
    Perishable = 1,
    [System.ComponentModel.DataAnnotations.Display(Name = "贵重")]
    Valuable = 2,
    [System.ComponentModel.DataAnnotations.Display(Name = "危险")]
    Hazardous = 3,
}
