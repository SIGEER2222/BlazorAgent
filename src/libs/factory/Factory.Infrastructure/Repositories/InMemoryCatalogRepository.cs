using Factory.Domain.Entities;

namespace Factory.Infrastructure.Repositories;

public interface ICatalogRepository
{
    IEnumerable<Item> GetItems();
    IEnumerable<Recipe> GetRecipes();
}

public class InMemoryCatalogRepository : ICatalogRepository
{
    readonly List<Item> _items =
    [
        new("iron-ore", "Iron Ore", "raw", 100),
        new("iron-plate", "Iron Plate", "intermediate", 100)
    ];

    readonly List<Recipe> _recipes =
    [
        new("iron-plate-smelt",
            Inputs: [("iron-ore", 1)],
            Outputs: [("iron-plate", 1)],
            TimeSeconds: 1.0,
            PowerKw: 0.5)
    ];

    public IEnumerable<Item> GetItems() => _items;
    public IEnumerable<Recipe> GetRecipes() => _recipes;
}