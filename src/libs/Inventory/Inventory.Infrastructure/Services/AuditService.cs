using Inventory.Domain.Entities;
using Inventory.Infrastructure.Data;
using System.Text.Json;

namespace Inventory.Infrastructure.Services;

public interface ICurrentUser { string? Name { get; } }

public class AuditService
{
    readonly InventoryDb _db;
    readonly ICurrentUser _user;
    public AuditService(InventoryDb db, ICurrentUser user) { _db = db; _user = user; }

    public Task LogAsync(string operation, string entity, string? reference, object? data)
    {
        var json = data == null ? null : JsonSerializer.Serialize(data);
        _db.Db.Insertable(new AuditLog { Time = DateTime.UtcNow, User = _user.Name, Operation = operation, Entity = entity, Reference = reference, DataJson = json }).ExecuteCommand();
        return Task.CompletedTask;
    }
}