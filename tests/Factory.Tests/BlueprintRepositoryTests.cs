using Factory.Domain.Entities;
using Factory.Infrastructure.Repositories;
using Xunit;

public class BlueprintRepositoryTests
{
    [Fact]
    public void Save_And_Get_Blueprint()
    {
        var repo = new InMemoryBlueprintRepository();
        var bp = new Blueprint("bp-001", "Basic Smelter", new Dictionary<string, object>{{"beltSpeed", 10}}, "{}");
        repo.Save(bp);
        var got = repo.Get("bp-001");
        Assert.NotNull(got);
        Assert.Equal("Basic Smelter", got!.Name);
        Assert.Equal(10, (int)got.Parameters!["beltSpeed"]);
    }
}