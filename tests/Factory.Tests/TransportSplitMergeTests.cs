using Factory.Simulation.Engine;
using Xunit;

public class TransportSplitMergeTests
{
    [Fact]
    public void Priority_Split_Fills_First_Output_To_Capacity()
    {
        var input = new FlowEdge("iron-plate", capacityPerSecond: 100);
        var out1 = new FlowEdge("iron-plate", capacityPerSecond: 10);
        var out2 = new FlowEdge("iron-plate", capacityPerSecond: 10);
        var split = new SplitNode(input, new[] { out1, out2 }, SplitStrategy.PriorityFirst);

        input.Push(100);
        split.Process(1.0);

        Assert.Equal(10, out1.Buffer);
        Assert.Equal(10, out2.Buffer);
    }

    [Fact]
    public void Merge_Respects_Output_Capacity()
    {
        var in1 = new FlowEdge("iron-plate", 10);
        var in2 = new FlowEdge("iron-plate", 15);
        var outEdge = new FlowEdge("iron-plate", 20);
        var merge = new MergeNode(new[] { in1, in2 }, outEdge);

        in1.Push(10);
        in2.Push(15);
        merge.Process(1.0);

        Assert.Equal(20, outEdge.Buffer);
    }
}