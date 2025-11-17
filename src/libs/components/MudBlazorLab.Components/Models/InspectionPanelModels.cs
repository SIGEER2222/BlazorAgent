namespace MudBlazorLab.Components.Models;

public class DocHeader {
    public string Template { get; set; } = string.Empty;
    public string DocNo { get; set; } = string.Empty;
    public string ErpNo { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Creator { get; set; } = string.Empty;
}

public class ObjectRow {
    public string DocNo { get; set; } = string.Empty;
    public string ObjectType { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string Batch { get; set; } = string.Empty;
    public int Total { get; set; }
    public string SampleRateText { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Creator { get; set; } = string.Empty;
}

public class SnRow {
    public string Sn { get; set; } = string.Empty;
    public string Item { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
}
