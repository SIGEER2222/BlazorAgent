using System;

namespace RxLearn.Web.Application.State;

public interface IAppState<T>
{
  IObservable<T> State { get; }
  void Update(T next);
}
