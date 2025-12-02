## 目标与范围
- 使用 Blazor 构建交互式网页，演示并讲解 Rx.NET 核心概念：Observable/Observer、Subject 系列、常用 Operators、Schedulers、错误与背压、生命周期管理。
- 每个概念配独立页面与可操作控件，支持实时可视化与源代码片段展示。

## 信息架构
- 首页：概览与导航（概念索引）。
- 冷/热 Observable：`Observable.Interval` 与 `Subject` 对比；`FromEventPattern` 示例。
- Subject 专题：`Subject`、`BehaviorSubject`、`ReplaySubject`、`AsyncSubject` 行为差异演示。
- Operators 专题：`Select`、`Where`、`Merge`、`CombineLatest`、`Switch`、`Scan` 交互式组合流。
- Schedulers：`SubscribeOn` vs `ObserveOn`，后台计算与 UI 更新演示。
- 错误与重试：`Throw`、`Catch`、`Retry`，可注入错误源与重试策略。
- 背压与节流：`Buffer`、`Window`、`Throttle`、`Debounce` 处理事件风暴。
- 资源与生命周期：`CompositeDisposable`、`Dispose`、完成与取消。

## 技术实现
- 依赖：在 `RxLearn.Web` 添加 `System.Reactive` 相关包；`RxLearn.Components` 复用 UI。
- 目录分层：`Application`（接口）/`Infrastructure`（实现）/`Web`（页面）。
- Reactive 服务：
  - 时钟源：`IDemoClockService`/`DemoClockService`，基于 `Observable.Interval` 暴露 `IObservable<long>`。
  - 事件桥：`IEventBridgeService`，用 `FromEventPattern` 将 DOM/Blazor 事件转为流。
  - SubjectHub：`ISubjectHub` 管理多类 `Subject` 并对外暴露只读流。
- 状态 Store：`IAppState<T>` 使用 `BehaviorSubject<T>`；对外 `IObservable<T>`。
- 组件模式：页面注入服务，`ObserveOn(SynchronizationContext.Current)` 订阅，`CompositeDisposable` 管理释放。

## 最小代码骨架
- 时钟源接口与实现：
```csharp
public interface IDemoClockService { IObservable<long> Ticks { get; } }
public sealed class DemoClockService : IDemoClockService {
  public IObservable<long> Ticks { get; } = Observable.Interval(TimeSpan.FromMilliseconds(500)).Publish().RefCount();
}
```
- SubjectHub：
```csharp
public interface ISubjectHub {
  IObservable<int> Plain { get; }
  IObservable<int> Behavior { get; }
  IObservable<int> Replay { get; }
  void Push(int value);
}
public sealed class SubjectHub : ISubjectHub {
  private readonly Subject<int> _plain = new();
  private readonly BehaviorSubject<int> _behavior = new(0);
  private readonly ReplaySubject<int> _replay = new(3);
  public IObservable<int> Plain => _plain.AsObservable();
  public IObservable<int> Behavior => _behavior.AsObservable();
  public IObservable<int> Replay => _replay.AsObservable();
  public void Push(int value) { _plain.OnNext(value); _behavior.OnNext(value); _replay.OnNext(value); }
}
```
- 页面订阅示例：
```csharp
@page "/subjects-demo"
@inject ISubjectHub Hub
@code {
  private readonly CompositeDisposable _disposables = new();
  private int _last;
  protected override void OnInitialized() {
    Hub.Behavior.ObserveOn(SynchronizationContext.Current!).Subscribe(v => { _last = v; StateHasChanged(); }).AddTo(_disposables);
  }
  public void Dispose() { _disposables.Dispose(); }
}
```
- DI 注册：
```csharp
builder.Services.AddSingleton<IDemoClockService, DemoClockService>();
builder.Services.AddSingleton<ISubjectHub, SubjectHub>();
```

## 交互与可视化
- 使用基础 Blazor 输入控件（按钮、滑块、文本框）触发事件流；展示当前值、历史窗口与合成流输出。
- 为 Operators 页面提供参数控制（阈值、时间窗口、合并选择），输出图表可先用简单列表与时间戳。

## 调度与错误处理
- 生产端：`SubscribeOn(TaskPoolScheduler.Default)`；UI 端：`ObserveOn(SynchronizationContext.Current)`。
- 错误路径：示例中提供注入异常按钮，服务侧 `Retry`、`Catch` 对比演示。

## 路由与导航
- 在 `Routes.razor` 配置专题页面路由；首页提供索引与链接跳转。

## 验证
- 完成 DI 与首页导航后，逐页实现与验证；每页交互确认行为正确，运行 `dotnet build` 验证编译。