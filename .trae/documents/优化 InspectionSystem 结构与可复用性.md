## 总体目标

* 降低视图与业务逻辑耦合、提升可复用性与可测试性

* 统一服务边界与数据访问方式，消除组件内直连数据库

* 通过视图模型管理状态与交互，组件保持“展示+绑定”职责

## 代码结构调整

* 目录统一：`Components/*` 仅 Razor；逻辑进入同名 `.razor.cs`

* 模型分层：`Model/Entities`、`Model/ViewModels`、`Model/DTOs`

* 服务分层：`Services/Domain`（业务）、`Services/Facade`（流程编排）、`Services/Infra`（仓储/适配）

## 组件拆分与绑定

* 完成 `.razor/.razor.cs` 拆分：`InspectionDocList`、`ObjectsPanel`、`DetailsPanel`（已进行）

* 继续拆分：`GridDataGridSelect.razor` 提取代码至 `.razor.cs` 并弱化内部状态逻辑

* 引入视图模型绑定：组件使用 VM（通过 `[Inject]` 或 `[CascadingParameter]`）而非直接字段管理

## 服务层改造

* 保留并强化现有服务：`IInspectionFormService`、`IInspectionObjectService`、`IInspectionDetailService`、`IInspectionConfigService`

* 新增 `IInspectionFacade`：聚合“创建表单→开始→生成对象→录入→结束”的完整流程，处理事务与一致性

* 扩展配置接口：已有 `GetTemplateNamesByFormTypeAsync`，补充按模板/对象类型粒度的查询方法返回 DTO

## 视图模型

* 新建 VM：`DocListViewModel`（分页/筛选/新建表单对话框状态）、`ObjectsPanelViewModel`（对象集合/新增对象对话框）、`DetailsPanelViewModel`（批次录入/校验）

* VM 仅依赖服务接口，不依赖 `SqlSugarClient`；组件通过参数或依赖注入绑定 VM

## 数据访问与事务

* 统一使用 `Repository<T>` 封装查询与分页，服务持有仓储

* Facade 内部应用事务边界（开始/结束、批次保存更新对象结果）

* 组件不再持有 `InspectionDb`，仅调用服务/VM 方法

## 交互与状态管理

* 所有 `SearchFunc(string, CancellationToken)` 支持取消；在服务端进行 TopN 过滤

* 统一结果返回为 `Result<T>`（或简单的 `(bool ok, string? error)`）供 UI 显示反馈（`MudSnackbar`/`MudDialog`）

* 细化状态：加载中/禁用按钮/错误提示统一管理在 VM

## 警告治理

* 消除可空警告：必填属性初始化或标记 `required`，对确可空字段改为可空类型

* 组件字段初始化与空检查（如 `InspectionObjectGrid` 等）

<br />

## 实施步骤

* 第1步：为 `GridDataGridSelect` 添加 `.razor.cs` 并迁移逻辑

* 第2步：新增三个 ViewModel 并切换 `InspectionDocList/ObjectsPanel/DetailsPanel` 到 VM 绑定

* 第3步：实现 `IInspectionFacade`，迁移跨组件流程调用到 Facade

* 第4步：治理空引用警告（初始化/可空标注）

* 第5步：补充单元测试，运行构建与基本交互验证

## 交付内容

* 新增 `Facade` 与 `ViewModels` 类文件，更新组件绑定与服务调用

* `GridDataGridSelect` 代码后置与状态简化

* 若干警告清理

