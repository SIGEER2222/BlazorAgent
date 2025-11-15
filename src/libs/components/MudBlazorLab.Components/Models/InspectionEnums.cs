namespace MudBlazorLab.Components.Models;

public enum InspectionType
{
    IQC,
    IPQC,
    OQC,
    Shift,
    Random
}

public enum DefectSeverity
{
    Critical,
    Major,
    Minor
}

public enum InspectionStatus
{
    Draft,
    InProgress,
    Completed,
    Rejected
}

