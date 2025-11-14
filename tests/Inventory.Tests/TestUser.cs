namespace Inventory.Tests.Helpers;

public class TestUser : Inventory.Infrastructure.Services.ICurrentUser
{
    public TestUser(string name) { Name = name; }
    public string? Name { get; }
}