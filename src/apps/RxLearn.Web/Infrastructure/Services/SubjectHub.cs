using System;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace RxLearn.Web.Infrastructure.Services;

public interface ISubjectHub
{
  IObservable<int> Plain { get; }
  IObservable<int> Behavior { get; }
  IObservable<int> Replay { get; }
  void Push(int value);
}

public sealed class SubjectHub : ISubjectHub
{
  private readonly Subject<int> plain = new();
  private readonly BehaviorSubject<int> behavior = new(0);
  private readonly ReplaySubject<int> replay = new(3);

  public IObservable<int> Plain => plain.AsObservable();
  public IObservable<int> Behavior => behavior.AsObservable();
  public IObservable<int> Replay => replay.AsObservable();

  public void Push(int value)
  {
    plain.OnNext(value);
    behavior.OnNext(value);
    replay.OnNext(value);
  }
}
