## 阶段汇总

### 阶段 1（目录、解决方案、项目与依赖）

* 解决方案与项目：已创建 `MudBlazorLab.sln`/`.slnx`，含 Web/RCL/bunit/Playwright 四个项目。

* 目标框架：统一为 `net9.0`（Web 调整为 `net9.0`，见 src/MudBlazorLab.Web/MudBlazorLab.Web.csproj:4）。

* 依赖：

  * Web：`MudBlazor 8.*`、`Serilog.AspNetCore`、`Serilog.Sinks.Console`、`Serilog.Sinks.File`、`Serilog.Settings.Configuration`。

  * RCL：`MudBlazor 8.*`。

  * bunit：`bunit`、`Microsoft.NET.Test.Sdk`、`xunit`、`xunit.runner.visualstudio`、`coverlet.collector`。

  * Playwright：`Microsoft.Playwright`、`Microsoft.NET.Test.Sdk`。

* 项目引用：Web→RCL，bunit→RCL；并生成 `.slnx`（dotnet sln migrate）。

### 阶段 2（运行与验证）

* 构建：`dotnet build MudBlazorLab.sln` 成功。

* 启动 Web：`dotnet run --project src/MudBlazorLab.Web --urls http://localhost:5070`；页面 `/hello` 可访问。

* 安装浏览器：`pwsh tests/MudBlazorLab.E2E/bin/Debug/net9.0/playwright.ps1 install`。

### 阶段 3（示例组件 + bunit/Playwright 测试）

* 组件：`src/MudBlazorLab.Components/HelloMud.razor` 显示计数，按钮点击递增。

* 页面：`src/MudBlazorLab.Web/Pages/Hello.razor` 暴露 `/hello` 路由。

* bunit 测试：`tests/MudBlazorLab.ComponentTests/HelloMudTests.cs` 渲染组件并断言“计数：1”。

* Playwright 测试：`tests/MudBlazorLab.E2E/HelloPageE2E.cs` 端到端验证点击与页面文本；使用 `BASE_URL`（示例：`http://localhost:5070`）。

* 集成要点：

  * Serilog：`src/MudBlazorLab.Web/Program.cs:8-12` 与 `appsettings.json`。

  * MudBlazor：服务注册 `Program.cs:18`，Provider `App.razor:1-5`，样式/脚本 `Pages/_Host.cshtml:15-16,35`，命名空间 `src/MudBlazorLab.Web/_Imports.razor:11-12`、`src/MudBlazorLab.Components/_Imports.razor:2`。

### 阶段 4（流程总结）

* 规范：组件集中 RCL；日志集中配置；E2E 使用 `BASE_URL` 环境变量；bunit 负责组件级、Playwright 负责端到端。

* 扩展：建议加入统一版本管理（props）、CI 流水线、`README` 使用说明。

## 提交与文档增强计划

### 1. 添加 README.md（中文）

* 内容包含：

  * 项目结构与分层说明

  * 快速开始：构建与运行 Web、安装浏览器、运行 bunit 与 Playwright

  * 环境变量：`BASE_URL` 使用说明

  * 常见问题：端口差异（以 `launchSettings.json` 为准）与 Playwright 浏览器安装

  * 参考链接：MudBlazor、Playwright .NET、bunit、.slnx 介绍

### 2. 统一版本与工程设置（可选）

* 新增 `build/Directory.Build.props`：

  * 锁定关键包版本（例如 `MudBlazor ~8.14.*`、`Microsoft.Playwright 1.56.0`、`bunit 2.0.66`、`Microsoft.NET.Test.Sdk 18.0.1` 等）

  * 统一 `TreatWarningsAsErrors` 与 `Nullable` 策略（保持与项目相容）

### 3. 仓库忽略文件

* 新增 `.gitignore`：忽略 `bin/`, `obj/`, `Logs/`, `TestResults/` 等。

### 4. CI 工作流（GitHub Actions）

* 新增 `.github/workflows/ci.yml`：

  * 设置 .NET 9 SDK

  * 还原与构建解决方案

  * 安装 Playwright 浏览器（执行 `pwsh tests/MudBlazorLab.E2E/bin/Debug/net9.0/playwright.ps1 install`，如需先构建 E2E 项目）

  * 运行 `dotnet test`（组件测试与 E2E 测试），通过 `BASE_URL` 连接到 `http://localhost:5070`（可配合 `aspnetcore` 自托管或使用 `Microsoft.AspNetCore.Mvc.Testing` 进阶实现）

### 5. 提交与信息

* 使用一次提交，信息示例：

  * `chore: add README, props, gitignore, CI; enable MudBlazor/Serilog; add sample tests`

### 6. 不做业务代码变更

* 本次仅新增文档与工程/CI文件，不触碰业务逻辑；确保与现有实现兼容。

## 运行提示与验证

* Web 启动：`dotnet run --project src/MudBlazorLab.Web --urls http://localhost:5070`

* 组件测试：`dotnet test tests/MudBlazorLab.ComponentTests -v minimal`

* E2E 测试：`$env:BASE_URL="http://localhost:5070"; dotnet test tests/MudBlazorLab.E2E -v minimal`

## 参考

* .slnx 支持（.NET SDK 9.0.200+）：<https://devblogs.microsoft.com/dotnet/introducing-slnx-support-dotnet-cli/>

* Playwright .NET：安装与浏览器 <https://playwright.dev/dotnet/docs/intro> <https://playwright.dev/dotnet/docs/browsers>

* bunit 创建测试项目：<https://bunit.dev/docs/getting-started/create-test-project.html> <https://www.nuget.org/packages/bunit/>

* MudBlazor 8.\*（.NET 8/9）：<https://www.nuget.org/packages/MudBlazor/latest>

