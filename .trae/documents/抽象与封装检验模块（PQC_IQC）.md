## 问题诊断
- 两套 DocList（PQC/IQC）与 ObjectsPanel 存在重复：数据网格、筛选、对话框状态、搜索方法、Start/End 操作、保存流程。
- 逻辑型重复分散在多个 .razor.cs 中，扩展困难，违背复用与单一职责。

## 重构目标
- 提供统一的列表与对象面板抽象，差异通过模板方法/策略注入。
- 复用通用筛选、分页、操作列与保存流程，派生类仅描述差异。
- 保持现有 MudBlazor 交互与服务接口不变，最小侵入地替换接入点。

## 技术方案
### 1) DocList 公共基类
- 新建 `InspectionCommon/DocListBase.razor` + `.razor.cs`
- 封装：
  - 通用网格（列：模板、单号、状态、时间、创建人、操作列）
  - 工具栏与“新增表单”按钮
  - 公共对话框字段：模板、单号、创建人、创建日期、是否立即开始
  - 通用筛选：`FilterHelper.And(typeFilter, predicate)`
  - 通用 Start/End 操作、`Facade.CreateFormFlowAsync`
- 通过抽象扩展：
  - `protected abstract FormTable GetFormType()`
  - `protected abstract RenderFragment RenderExtraFields()`（PQC：产线+工单；IQC：ERP 单号）
  - `protected abstract void MapExtraToForm(InspectionForm form)`（PQC 写 `WorkCenter/ProductName`；IQC 写 `TicketNo`）
  - `protected virtual bool ValidateExtra()`（PQC 要求 `WorkOrders.Any()`；IQC 要求 `Tickets.Any()`）

### 2) PQC/IQC DocList 派生类
- `ProcessDocList : DocListBase`
  - 实现 `GetFormType=过程检验单`
  - 额外字段：产线 Autocomplete + `GridDataGridSelect<fab_work_order>`
  - 校验与映射：写 `ProductName/WorkCenter`
- `IncomingDocList : DocListBase`
  - 实现 `GetFormType=来料检验单`
  - 额外字段：`GridDataGridSelect<ErpTicket>`
  - 校验与映射：写 `TicketNo`

### 3) ObjectPanel 公共基类
- 新建 `InspectionCommon/ObjectPanelBase.razor` + `.razor.cs`
- 封装：
  - 通用网格（对象类型、名称、结果、创建日期、创建人）
  - 通用行为：加载对象/视图、点击行、关闭面板
  - 通用“新增对象”按钮与提交管道
- 通过抽象扩展：
  - `protected abstract RenderFragment RenderAddDialog()`（PQC：载具+批次；IQC：批次+总量+抽样率）
  - `protected abstract Task SaveObjectAsync()`（PQC 调用含载具/批次；IQC 计算抽样数后保存）
  - 可选列扩展：`protected virtual IEnumerable<ColumnDef> ExtraColumns()`（PQC 增加载具/批次；IQC 增加批次/总量/抽样率/抽样数）
- 派生：`ProcessObjectsPanel : ObjectPanelBase`、`IncomingObjectsPanel : ObjectPanelBase`

### 4) 通用列/操作复用
- 将操作列抽成 `DocActionColumn`（或基类中内置），复用 `StartForm/EndForm`。
- 保留现 `FilterHelper.And(...)`，删除组件内自定义 AndAlso/Visitor。

### 5) 依赖与接口保持不变
- 继续使用现有 `IInspectionConfigService`、`IInspectionFormService`、`IInspectionFacade`、`IInspectionObjectService`。
- 新基类内部封装 `Repository<InspectionForm>.LoadGridDataAsync(...)` 调用与过滤拼接。

## 文件结构调整
- `SystemModule/InspectionSystem/Components/InspectionCommon/DocListBase.*`
- `SystemModule/InspectionSystem/Components/InspectionCommon/ObjectPanelBase.*`
- `ProcessInspection/ProcessDocList` 与 `IncomingInspection/IncomingDocList` 简化为派生类，只实现差异。

## 实施步骤
1. 引入 `DocListBase` 与 `ObjectPanelBase`，复制通用 UI/逻辑。
2. 精简 `ProcessDocList`、`IncomingDocList` 至派生实现：额外字段、校验与映射。
3. 精简 `ProcessObjectsPanel`、`IncomingObjectsPanel` 至派生实现：新增对象弹窗与保存逻辑调用现服务。
4. 删除各组件内自定义表达式合并与重复操作列代码。
5. 运行构建与页面联调，确保三类选项卡（过程/来料/开班点检）工作正常。

## 验证与回滚
- 验证：构建通过、三类列表增删查与“开始/结束”操作正常，对象新增与明细面板可打开。
- 回滚策略：保留原组件文件历史，若需回退仅切换引用为原 `*DocList` 与 `*ObjectsPanel`。

请确认是否按此方案开始重构并落地实现。