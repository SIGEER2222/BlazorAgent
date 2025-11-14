## 项目背景与现状

* 现有结构：`MudBlazorLab.Web`（Web 主站）、`MudBlazorLab.Components`（RCL）、测试项目（bUnit/Playwright）

* 技术栈：ASP.NET Core Razor Components（交互式服务器渲染）、MudBlazor UI、Cookie 认证与基于策略的授权

* RCL 能力：`FeatureGuard`/`PolicyGuard` 权限渲染、通用组件（如 `AutoDataGrid`）、模型与服务

* 数据层：暂未接入数据库/ORM，演示数据以内存集合为主；已有 `MiniExcel`（导出/导入）与 `Bogus`（数据生成）

## 目标与范围（MVP）

* 建立一个可用的进销存（采购-销售-库存）系统，覆盖基础资料、单据流转、库存核算、常用报表与权限控制

* 面向单仓或多仓，支持常规业务流程与移动加权平均成本法（简化版）

## 系统架构与选型

* 前端：Razor Components（Server 交互模式），复用现有 MudBlazor 主题与 RCL 组件

* 共享库：直接引用现有 `MudBlazorLab.Components`，扩展权限映射与通用 UI

* 数据层：sqlsugar（开发默认 SQLite，生产可切换 SQL Server）；分层为 Domain、Infrastructure、Application、UI

* 身份与授权：沿用 Cookie 认证与策略授权；用 `FeatureGuard`/`PolicyGuard` 精准控制菜单、按钮、页面

## 核心数据模型（初版）

* 基础资料：`Product`、`Category`、`Unit`、`Supplier`、`Customer`、`Warehouse`、`Location`（可选）、`PriceList`

* 采购：`PurchaseOrder`、`PurchaseOrderLine`、`GoodsReceipt`、`GoodsReceiptLine`

* 销售：`SalesOrder`、`SalesOrderLine`、`GoodsIssue`、`GoodsIssueLine`

* 库存：`InventoryMovement`（统一流水：入/出/调拨/盘盈盘亏）、`StockBalance`（按仓/货位/批次）

* 账务（简化）：`CostLayer`（移动加权平均），`Settlement`（可选）

## 业务流程与约束

* 采购：PO 审批→收货→入库 → 生成库存流水与成本更新

* 销售：SO 审批→拣选/出库 → 生成库存流水与成本结转

* 调拨：跨仓移库，形成两条流水（出库+入库）并保持事务一致性

* 盘点：盘点任务→差异计算→盘盈/盘亏调整，更新库存与成本

* 约束：库存不可为负（可配置允许负库存/预占）；单据状态机（草稿→审核→完成→作废）

## 权限设计

* 角色示例：`Admin`、`Manager`、`Editor`、`Viewer`

* 特性映射：`Inventory.View`、`Inventory.Edit`、`Purchase.Create`、`Sales.Create`、`MasterData.Manage`、`Report.View`

* 页面/组件：通过现有 `FeatureGuard`/`PolicyGuard` 控制渲染与行为；策略绑定到路由与操作

## 报表与导出

* 库存报表：库存余额表、库存流水明细、资金占用（按品类/仓库汇总）

* 采购/销售：未完成订单、供应商/客户统计、毛利分析（简化）

* 导出：复用 `MiniExcel` 导出 Excel；导入支持基础资料与价格表

## 实现步骤（12 步）

1. 新建 `Inventory.Web` 项目并接入现有解决方案结构；引用 `MudBlazorLab.Components`（RCL）
2. 建立统一布局与导航（侧边栏/顶部栏），接入现有主题与通用组件
3. 引入 sqlsugar（SQLite 开发环境），配置 `DbContext`、连接串
4. 定义基础资料实体与仓储接口，完成 CRUD 页面与验证
5. 设计库存流水与结存模型（`InventoryMovement`/`StockBalance`），实现移动加权平均成本更新服务
6. 采购模块：PO 建立/审批，收货与入库；联动库存与成本
7. 销售模块：SO 建立/审批，拣选与出库；联动库存与成本
8. 调拨与盘点：跨仓移库与盘盈盘亏调整；事务一致性与差错处理
9. 报表中心：余额/流水/订单与毛利报表，支持筛选与导出
10. 权限接入：定义特性与策略，菜单/页面/按钮按权限渲染；审计日志（可选）
11. 测试保障：bUnit 组件测试、核心服务单元测试、Playwright 端到端场景
12. 部署与运维：连接串与迁移脚本、种子数据、基础监控与日志

## 数据一致性与事务

* 所有单据过账操作包裹在事务中，保证库存与成本一致更新

* 采用乐观并发控制（行版本）；关键表加唯一约束（如同仓同货品批次唯一）

## 成本核算策略（MVP）

* 移动加权平均：每次入库计算新平均成本；出库按当前平均成本结转

* 后续可扩展：FIFO/LIFO、批次成本与序列号管理

## 交互体验与可用性

* 表格：分页、排序、筛选、列选择；支持快速编辑与只读模式

* 表单：分步引导、校验提示、自动保存草稿；引用通用表单控件

* 搜索与选择：供应商/客户/货品智能选择框；支持条码（可选）

## 里程碑与交付

* 里程碑A（基础骨架与资料）：步骤1-4，交付基础资料与可运行站点

* 里程碑B（库存与单据）：步骤5-8，交付完整采购/销售/库存流程

* 里程碑C（报表与测试）：步骤9-12，交付报表、测试与部署资产

## 需要您确认的选项

* 数据库：默认 SQLite（开发）

* 成本法：MVP 采用移动加权平均，需要 FIFO

* 初始角色与权限：沿用现有 `Admin/Manager/Editor/Viewer`

* 多仓与货位：MVP 启用货位管理

