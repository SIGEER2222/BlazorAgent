namespace Factory.Simulation.Engine;

public enum SplitStrategy { PriorityFirst, RoundRobin }

public class FlowEdge
{
    public string ItemId { get; }
    public double CapacityPerSecond { get; }
    double _buffer = 0;
    public FlowEdge(string itemId, double capacityPerSecond)
    {
        ItemId = itemId; CapacityPerSecond = capacityPerSecond;
    }
    public double Buffer => _buffer;
    public double Push(double amount)
    {
        _buffer += amount; return _buffer;
    }
    public double Drain(double seconds)
    {
        var maxOut = CapacityPerSecond * seconds;
        var outAmt = Math.Min(_buffer, maxOut);
        _buffer -= outAmt; return outAmt;
    }
}

public class SplitNode
{
    readonly FlowEdge _input;
    readonly FlowEdge[] _outputs;
    readonly SplitStrategy _strategy;
    int _rrIndex = 0;
    public SplitNode(FlowEdge input, FlowEdge[] outputs, SplitStrategy strategy)
    { _input = input; _outputs = outputs; _strategy = strategy; }

    public void Process(double seconds)
    {
        var incoming = _input.Drain(seconds);
        if (incoming <= 0) return;
        switch (_strategy)
        {
            case SplitStrategy.PriorityFirst:
                foreach (var o in _outputs)
                {
                    if (incoming <= 0) break;
                    var accept = Math.Min(incoming, o.CapacityPerSecond * seconds);
                    o.Push(accept);
                    incoming -= accept;
                }
                break;
            case SplitStrategy.RoundRobin:
                var i = _rrIndex;
                while (incoming > 0)
                {
                    var o = _outputs[i % _outputs.Length];
                    var accept = Math.Min(incoming, o.CapacityPerSecond * seconds);
                    if (accept <= 0) break;
                    o.Push(accept);
                    incoming -= accept;
                    i++;
                }
                _rrIndex = i;
                break;
        }
    }
}

public class MergeNode
{
    readonly FlowEdge[] _inputs;
    readonly FlowEdge _output;
    public MergeNode(FlowEdge[] inputs, FlowEdge output)
    { _inputs = inputs; _output = output; }

    public void Process(double seconds)
    {
        var capacity = _output.CapacityPerSecond * seconds;
        var remaining = capacity;
        foreach (var inp in _inputs)
        {
            if (remaining <= 0) break;
            var pulled = inp.Drain(seconds);
            var accept = Math.Min(remaining, pulled);
            _output.Push(accept);
            remaining -= accept;
        }
    }
}