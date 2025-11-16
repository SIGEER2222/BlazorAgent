using Factory.Domain.Entities;

namespace Factory.Simulation.Engine;

public class SimulationEngine
{
    readonly List<Machine> _machines = new();
    readonly Dictionary<string, double> _inventory = new();
    readonly Dictionary<string, double> _produced = new();
    double _totalEnergyKWh = 0;
    public double PowerDomainKw { get; set; } = 1000;
    public int Tps { get; }
    public bool Running { get; private set; }
    public double ElapsedSeconds { get; private set; }
    public SimulationEngine(int tps = 60) { Tps = tps; }

    public void AddMachine(Machine m) => _machines.Add(m);
    public IReadOnlyDictionary<string, double> Inventory => _inventory;
    public IReadOnlyDictionary<string, double> Produced => _produced;
    public double TotalEnergyKWh => _totalEnergyKWh;

    public void Start() => Running = true;
    public void Stop() => Running = false;

    public void Step(double seconds, IEnumerable<Recipe> recipes)
    {
        var totalPowerKw = recipes.Sum(r => r.PowerKw);
        var scale = totalPowerKw > PowerDomainKw && totalPowerKw > 0 ? PowerDomainKw / totalPowerKw : 1.0;
        var effectiveSeconds = seconds * scale;
        ElapsedSeconds += effectiveSeconds;
        foreach (var r in recipes)
        {
            var timeBatches = (int)Math.Floor(effectiveSeconds / r.TimeSeconds);
            if (timeBatches <= 0) continue;
            var maxByInputs = r.Inputs.Select(i => (int)Math.Floor(_inventory.GetValueOrDefault(i.itemId) / i.amount)).DefaultIfEmpty(0).Min();
            var batches = Math.Min(timeBatches, maxByInputs);
            if (batches <= 0) continue;
            foreach (var i in r.Inputs)
                _inventory[i.itemId] = _inventory.GetValueOrDefault(i.itemId) - i.amount * batches;
            foreach (var o in r.Outputs)
            {
                var add = o.amount * batches;
                _inventory[o.itemId] = _inventory.GetValueOrDefault(o.itemId) + add;
                _produced[o.itemId] = _produced.GetValueOrDefault(o.itemId) + add;
            }
            var hours = r.TimeSeconds / 3600.0 * batches;
            _totalEnergyKWh += r.PowerKw * hours;
        }
    }

    public void Seed(string itemId, double amount) => _inventory[itemId] = _inventory.GetValueOrDefault(itemId) + amount;
}