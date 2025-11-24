using InspectionSystem.Models;

namespace InspectionSystem.Data;

public class WorkOrderDto {
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class DetailInput {
    public bool Selected { get; set; }
    public string CheckObject { get; set; } = string.Empty;
    public string CheckItem { get; set; } = string.Empty;
    public string CheckDescription { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal? NumericValue { get; set; }
    public InspectionResult Result { get; set; } = InspectionResult.OK;
}