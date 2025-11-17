using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace MudBlazorLab.Web.Services;

public interface IRabbitMQMessageService {
    IObservable<QueenInfoMessage> Messages { get; }
    IObservable<IReadOnlyList<QueenInfoMessage>> Buffered { get; }
}

public class RabbitMQMessageService : IRabbitMQMessageService {
    private readonly RabbitMQConsumerService _consumerService;
    private const int MaxBufferedMessages = 1000;
    private IObservable<QueenInfoMessage>? _replayed;
    private IObservable<IReadOnlyList<QueenInfoMessage>>? _buffered;

    public RabbitMQMessageService(RabbitMQConsumerService consumerService) {
        _consumerService = consumerService;
        // Create a replayed stream of last 1000 messages using Rx operators
        _replayed = _consumerService.Messages.Replay(bufferSize: MaxBufferedMessages).RefCount();
        // Aggregate into a capped list with Scan; new subscribers receive replay then continue
        _buffered = _replayed.Scan((IReadOnlyList<QueenInfoMessage>)new List<QueenInfoMessage>().AsReadOnly(), (acc, msg) => {
            var next = acc.ToList();
            next.Insert(0, msg);
            if (next.Count > MaxBufferedMessages) next.RemoveAt(next.Count - 1);
            return (IReadOnlyList<QueenInfoMessage>)next.AsReadOnly();
        });
    }

    public IObservable<QueenInfoMessage> Messages => _replayed!;
    public IObservable<IReadOnlyList<QueenInfoMessage>> Buffered => _buffered!;
}
