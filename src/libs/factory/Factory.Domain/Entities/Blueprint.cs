namespace Factory.Domain.Entities;

public record Blueprint(
    string Id,
    string Name,
    Dictionary<string, object>? Parameters,
    string? Json
);