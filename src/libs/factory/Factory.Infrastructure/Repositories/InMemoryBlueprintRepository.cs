using Factory.Domain.Entities;

namespace Factory.Infrastructure.Repositories;

public interface IBlueprintRepository
{
    void Save(Blueprint bp);
    Blueprint? Get(string id);
}

public class InMemoryBlueprintRepository : IBlueprintRepository
{
    readonly Dictionary<string, Blueprint> _store = new();
    public void Save(Blueprint bp) => _store[bp.Id] = bp;
    public Blueprint? Get(string id) => _store.TryGetValue(id, out var bp) ? bp : null;
}