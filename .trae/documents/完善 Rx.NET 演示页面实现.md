## 范围
- 完成剩余专题页面的可交互实现：Cold/Hot、Operators、Schedulers、Errors、Backpressure、Lifecycle。
- 保持现有分层：服务内组合流、组件只订阅；统一 UI 线程调度与释放。

## Cold vs Hot（/cold-hot）
- 展示：两个订阅者分别订阅冷源 (`Observable.Interval`) 与热源 (`Interval.Publish().RefCount()`) 的差异。
- 交互：按钮新增订阅、取消订阅；显示每个订阅的最新值与时间戳。
- 要点：冷源每次订阅从 0 开始；热源共享进度。

## Operators（/operators-demo）
- 演示组合：
  - `Select` 映射 ticks→平方；`Where` 过滤偶数；
  - `Merge` 合并两按钮点击流；`CombineLatest` 合成两个滑块；
  - `Switch` 切换最新选择的源；`Scan` 累积和；`Buffer` 按时间/数量批次。
- 交互：滑块/输入框/按钮控制参数与源选择；以列表显示输出。

## Schedulers（/schedulers-demo）
- 展示：`SubscribeOn(TaskPoolScheduler.Default)` 后台计算，`ObserveOn(SynchronizationContext.Current)` 更新 UI。
- 交互：按钮触发耗时计算（模拟 `Select` 中 `Thread.Sleep`/`Task.Delay`），可视化调度对比。

## Errors（/errors-demo）
- 演示：源在第 N 次后 `Throw`；对比 `Catch` 恢复与 `Retry(count)` 重试；`Materialize` 展示 `OnError/OnCompleted`。
- 交互：设置 N 与重试次数；按钮触发，列表展示事件序列。

## Backpressure（/backpressure-demo）
- 演示：高频点击/输入事件流，用 `Throttle/Debounce/Sample/Buffer/Window` 控制节流与分批。
- 交互：滑块设置窗口大小与间隔；列表展示分批结果与时间戳。

## Lifecycle（/lifecycle-demo）
- 展示：组件级订阅的创建与释放；服务侧 `OnCompleted()` 与观察者反应。
- 交互：按钮创建/销毁订阅、完成流；显示完成状态与是否继续推送。

## 技术要点
- 组件订阅统一：`ObserveOn(SynchronizationContext.Current)`；在 `Dispose()` 释放。
- 导入：在需要的页面使用 `@using System.Reactive.Linq` 与 `@using System.Reactive.Concurrency`。
- 复用 `DemoClockService`/`SubjectHub` 作为演示源；页面内可用 `Publish().RefCount()` 派生热流。

## 验证
- 每页实现后 `dotnet build` 验证编译；通过导航访问交互，确认订阅、调度、错误与背压行为正确。