namespace Factory.Domain.Entities;

public record Recipe(
    string Id,
    (string itemId, int amount)[] Inputs,
    (string itemId, int amount)[] Outputs,
    double TimeSeconds,
    double PowerKw);