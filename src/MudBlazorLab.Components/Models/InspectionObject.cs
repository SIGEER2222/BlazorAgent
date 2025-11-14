using System;
using MudBlazorLab.Components.Models;

namespace MudBlazorLab.Components.Models;

public class InspectionObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public InspectionType Type { get; set; }
    public string? Supplier { get; set; }
    public string? PurchaseOrder { get; set; }
    public string? MaterialCode { get; set; }
    public string? MaterialDesc { get; set; }
    public string? BatchNo { get; set; }
    public int Quantity { get; set; }
    public string? ProductionOrder { get; set; }
    public string? Line { get; set; }
    public string? Station { get; set; }
    public string? Shift { get; set; }
    public string? Customer { get; set; }
    public string? ShipmentNo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public InspectionStatus Status { get; set; } = InspectionStatus.Draft;
}

