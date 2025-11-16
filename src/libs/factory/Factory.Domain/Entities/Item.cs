namespace Factory.Domain.Entities;

public record Item(string Id, string Name, string Category, int StackSize = 100);