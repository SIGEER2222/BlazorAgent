namespace InspectionSystem.Models;

public class InspectionFlatRow
{
    public string SampleBatchNo { get; set; } = string.Empty;
    public int BatchIndex { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
    public string? Unit { get; set; }
    public decimal? Value { get; set; }
    public string CheckResult { get; set; } = string.Empty;
    public string BatchCheckResult { get; set; } = string.Empty;
}
