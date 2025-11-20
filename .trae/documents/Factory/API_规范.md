API 规范（首版模板）

目标
- 统一接口风格、错误码与分页过滤，保障幂等与兼容。
- 明确鉴权与权限边界，便于上线与演进。

版本策略
- 路径版本：`/api/v1/...`（M1 使用无版本路径，M2 起引入）。
- 兼容窗口：次要字段新增采用“可选 + 默认”，重大变更以次版本发布并保留旧版≥2个迭代。

资源命名与路径
- 资源名小写中划线；集合资源用复数，子资源采用嵌套：`/api/blueprints/{id}`、`/api/factories/{id}/deploy`。
- 现有端点：`src/apps/Factory.Web/Program.cs:39-44`、`src/apps/Factory.Web/Program.cs:46-47`、`src/apps/Factory.Web/Program.cs:49-67`。

分页与过滤
- 查询参数：`page`（默认1）、`pageSize`（默认20，≤100）、`sort`、`order`（`asc|desc`）。
- 过滤：`q`（全文）、`filter.field=value`（精确）。返回含分页元数据：`{ data:[], page, pageSize, total }`。

错误响应
- 结构：
  ```json
  {
    "error": {
      "code": "catalog.invalid",
      "message": "字段不合法",
      "details": [{ "field": "items[0].id", "issue": "重复" }]
    },
    "traceId": "..."
  }
  ```
- 错误码分层：`auth.*`、`permission.*`、`catalog.*`、`blueprint.*`、`sim.*`、`internal.*`。

幂等性
- 读取（GET）天然幂等；
- 写入：POST 蓝图创建在客户端可使用 `Idempotency-Key` 头，服务器记录 24h；PUT/PATCH 按资源版本号（`etag` 或 `version` 字段）冲突检测。

速率限制
- 建议对匿名与普通用户设定基础配额（如 60 req/min），对批量导入端点单独限制；返回 `429` 并附带 `Retry-After`。

鉴权与权限
- M1 无鉴权；M2 起引入 Bearer（JWT）或 Cookie 会话之一。
- 角色建议与来源：`类异星工厂系统总纲.md:17-22`（工程师/经理/管理员/观察者）。
- 端点权限建议：
  - 只读：`GET /api/items`、`GET /api/recipes`、`GET /api/sim/metrics` → 所有登录用户。
  - 写入：`POST /api/blueprints`、`POST /api/factories/{id}/deploy` → 工程师及以上。

签名与时间戳（可选）
- 外部分享/扩展调用建议启用请求签名（HMAC）与时间戳窗口，防重放。

可观测性
- 为每次请求生成 `traceId`，写入结构化日志与响应；日志建议扩展至含 `UserId/Action/Resource/Result`（可参考 `src/libs/components/MudBlazorLab.Components/Models/LogEntry.cs:12-18` 并扩展字段）。

接口清单映射（当前实现）
- 仿真指标：`/api/sim/metrics` → `src/apps/Factory.Web/Program.cs:39-44`
- 目录：`/api/items`、`/api/recipes` → `src/apps/Factory.Web/Program.cs:46-47`
- 蓝图：`POST /api/blueprints`、`GET /api/blueprints/{id}`、`POST /api/factories/{id}/deploy` → `src/apps/Factory.Web/Program.cs:49-67`

变更管理
- 每次端点新增/变更，更新此清单并在发布说明记录兼容策略与迁移指引。

