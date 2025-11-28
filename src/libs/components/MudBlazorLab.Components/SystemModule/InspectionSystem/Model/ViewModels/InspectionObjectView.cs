using System;

namespace InspectionSystem.Models;

public sealed class InspectionObjectView
{
    public Guid Sysid { get; set; }
    public string ObjectType { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string? CarrierName { get; set; }
    public string? SampleBatchNo { get; set; }
    public long? TotalQuantity { get; set; }
    public decimal? ActualSamplingRatio { get; set; }
    public long? SampleQuantity { get; set; }
    public string CheckResult { get; set; } = string.Empty;
    public string FormTemplateName { get; set; } = string.Empty;
    public string FormNo { get; set; } = string.Empty;
    public string WorkCenter { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public string CreateUser { get; set; } = string.Empty;
    public string ObjectDisplayName => string.IsNullOrWhiteSpace(CheckResult) ? ObjectName : $"{ObjectName} ({CheckResult})";
}
