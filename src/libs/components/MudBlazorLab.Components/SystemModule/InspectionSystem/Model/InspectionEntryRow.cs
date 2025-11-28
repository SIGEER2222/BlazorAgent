namespace InspectionSystem.Models;

public class InspectionEntryRow
{
    public string ItemName { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
    public string? Unit { get; set; }
    public string? ValueText { get; set; }
    public string CheckResult { get; set; } = "OK";
}
