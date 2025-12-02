using System;
using System.Reactive.Linq;

namespace RxLearn.Web.Infrastructure.Services;

public interface IDemoClockService
{
  IObservable<long> Ticks { get; }
}

public sealed class DemoClockService : IDemoClockService
{
  public IObservable<long> Ticks { get; } =
    Observable.Interval(TimeSpan.FromMilliseconds(500)).Publish().RefCount();
}
