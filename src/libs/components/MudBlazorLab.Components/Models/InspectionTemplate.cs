using System.Collections.Generic;

namespace MudBlazorLab.Components.Models;

public class InspectionTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "v1";
    public InspectionType Type { get; set; }
    public string InspectionLevel { get; set; } = "II";
    public double AqlCritical { get; set; } = 0.0;
    public double AqlMajor { get; set; } = 2.5;
    public double AqlMinor { get; set; } = 4.0;
    public List<InspectionItem> Items { get; set; } = new();
}

public class InspectionItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public bool Required { get; set; }
    public string? Threshold { get; set; }
}

