using MudBlazorLab.Components.Models;

namespace MudBlazorLab.Components.Models;

public class Defect
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DefectSeverity Severity { get; set; }
    public string Code { get; set; } = string.Empty;
    public int Count { get; set; }
    public string? Description { get; set; }
}

