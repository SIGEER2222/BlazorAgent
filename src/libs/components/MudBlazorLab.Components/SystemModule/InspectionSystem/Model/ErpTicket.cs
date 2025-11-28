using System;
namespace InspectionSystem.Models;

public sealed class ErpTicket
{
    public string TicketNo { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}
