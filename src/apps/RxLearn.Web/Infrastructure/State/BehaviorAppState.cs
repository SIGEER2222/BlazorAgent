using System;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using RxLearn.Web.Application.State;

namespace RxLearn.Web.Infrastructure.State;

public sealed class BehaviorAppState<T> : IAppState<T>
{
  private readonly BehaviorSubject<T> subject;

  public BehaviorAppState()
  {
    subject = new BehaviorSubject<T>(default!);
  }

  public IObservable<T> State => subject.AsObservable();

  public void Update(T next)
  {
    subject.OnNext(next);
  }
}
