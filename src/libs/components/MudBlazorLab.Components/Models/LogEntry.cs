using System;
using System.ComponentModel.DataAnnotations;

namespace MudBlazorLab.Components.Models;

public enum LogMessageType {
    Info = 0,
    Error = 1,
    Alarm = 2
}

public class LogEntry {
    public DateTime Time { get; set; }
    public LogMessageType Type { get; set; }
    public string Line { get; set; } = string.Empty;
    public string ObjectType { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

