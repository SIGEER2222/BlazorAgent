using Factory.Domain.Entities;
using Factory.Simulation.Engine;
using Xunit;
using System.Linq;

public class PowerThrottlingTests
{
    [Fact]
    public void Throttles_When_Power_Demand_Exceeds_Domain()
    {
        var eng = new SimulationEngine();
        var recipes = new[]
        {
            new Recipe("iron-plate-smelt", new[]{ ("iron-ore",1) }, new[]{ ("iron-plate",1) }, 1.0, 0.5),
            new Recipe("steel-smelt", new[]{ ("iron-plate",2) }, new[]{ ("steel",1) }, 1.0, 1.0)
        };
        eng.Seed("iron-ore", 100);
        eng.Seed("iron-plate", 100);

        eng.PowerDomainKw = 10; // ample power
        eng.Step(10, recipes);
        var ampleProduced = eng.Produced.ToDictionary(k => k.Key, v => v.Value);

        var eng2 = new SimulationEngine();
        eng2.Seed("iron-ore", 100);
        eng2.Seed("iron-plate", 100);
        eng2.PowerDomainKw = 0.5; // tight power budget
        eng2.Step(10, recipes);
        var tightProduced = eng2.Produced.ToDictionary(k => k.Key, v => v.Value);

        Assert.True(tightProduced.GetValueOrDefault("steel") < ampleProduced.GetValueOrDefault("steel"));
        Assert.True(tightProduced.GetValueOrDefault("iron-plate") <= ampleProduced.GetValueOrDefault("iron-plate"));
    }
}