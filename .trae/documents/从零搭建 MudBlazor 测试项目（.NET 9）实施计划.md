## 概览

* 目标：基于 .NET 9 + MudBlazor 8.\*，集成 Serilog、Playwright、bunit，搭建用于组件验证的 Blazor 项目

* 架构：Blazor Server（交互式），基础组件集中在 Razor Class Library（RCL）

* 解决方案：使用 .sln 生成并迁移到 .slnx（CLI 支持于 .NET SDK 9.0.200，VS 17.14）

* 测试：bunit 做组件级单测；Playwright 做端到端 UI 测试

## 目录结构

```
MudBlazorLab.slnx
MudBlazorLab.sln
src/
  MudBlazorLab.Web/                # Blazor Server 应用（主站点）
  MudBlazorLab.Components/         # Razor Class Library（基础组件库，依赖 MudBlazor）
tests/
  MudBlazorLab.ComponentTests/     # bunit + xUnit 组件测试
  MudBlazorLab.E2E/                # Playwright + xUnit 端到端测试
build/
  Directory.Build.props            # 可选：统一包版本/警告级别（后续加入）
```

## 项目类型与依赖

* `MudBlazorLab.Web`：

  * 类型：Blazor Server，`net9.0`

  * 依赖：`MudBlazor` 8.\*、`Serilog.AspNetCore`、`Serilog.Sinks.Console`、`Serilog.Sinks.File`、`Serilog.Settings.Configuration`

* `MudBlazorLab.Components`（RCL）：

  * 类型：Razor Class Library，`net9.0`

  * 依赖：`MudBlazor` 8.\*

* `MudBlazorLab.ComponentTests`：

  * 类型：xUnit + bunit，`net9.0`

  * 依赖：`bunit`（2.x，支持 .NET 8+）、`Microsoft.NET.Test.Sdk`、`xunit`、`xunit.runner.visualstudio`、`coverlet.collector`

* `MudBlazorLab.E2E`：

  * 类型：xUnit + Playwright，`net9.0`

  * 依赖：`Microsoft.Playwright`、`Microsoft.NET.Test.Sdk`、`xunit`

## 阶段 1（创建目录、解决方案与项目、添加 NuGet 包）

* 创建解决方案与项目

  * `dotnet new sln -n MudBlazorLab`

  * `dotnet new mudblazor --interactivity Server -f net9.0`

  * `dotnet new razorclasslib -n MudBlazorLab.Components -f net9.0`

  * `dotnet new xunit -n MudBlazorLab.ComponentTests -f net9.0`

  * `dotnet new xunit -n MudBlazorLab.E2E -f net9.0`

  * 将项目加入解决方案：

    * `dotnet sln add src/MudBlazorLab.Web/MudBlazorLab.Web.csproj`

    * `dotnet sln add src/MudBlazorLab.Components/MudBlazorLab.Components.csproj`

    * `dotnet sln add tests/MudBlazorLab.ComponentTests/MudBlazorLab.ComponentTests.csproj`

    * `dotnet sln add tests/MudBlazorLab.E2E/MudBlazorLab.E2E.csproj`

  * 迁移到 `.slnx`：

    * `dotnet sln migrate`

* 添加项目引用

  * Web 引用组件库：

    * `dotnet add src/MudBlazorLab.Web/MudBlazorLab.Web.csproj reference src/MudBlazorLab.Components/MudBlazorLab.Components.csproj`

  * bunit 测试引用组件库：

    * `dotnet add tests/MudBlazorLab.ComponentTests/MudBlazorLab.ComponentTests.csproj reference src/MudBlazorLab.Components/MudBlazorLab.Components.csproj`

* 添加 NuGet 包

  * Web：

    * `dotnet add src/MudBlazorLab.Web package MudBlazor --version 8.*`

    * `dotnet add src/MudBlazorLab.Web package Serilog.AspNetCore`

    * `dotnet add src/MudBlazorLab.Web package Serilog.Sinks.Console`

    * `dotnet add src/MudBlazorLab.Web package Serilog.Sinks.File`

    * `dotnet add src/MudBlazorLab.Web package Serilog.Settings.Configuration`

  * 组件库：

    * `dotnet add src/MudBlazorLab.Components package MudBlazor --version 8.*`

  * bunit 测试：

    * `dotnet add tests/MudBlazorLab.ComponentTests package bunit`

    * `dotnet add tests/MudBlazorLab.ComponentTests package Microsoft.NET.Test.Sdk`

    * `dotnet add tests/MudBlazorLab.ComponentTests package xunit`

    * `dotnet add tests/MudBlazorLab.ComponentTests package xunit.runner.visualstudio`

    * `dotnet add tests/MudBlazorLab.ComponentTests package coverlet.collector`

  * Playwright 测试：

    * `dotnet add tests/MudBlazorLab.E2E package Microsoft.Playwright`

    * `dotnet add tests/MudBlazorLab.E2E package Microsoft.NET.Test.Sdk`

* Serilog 初始集成

  * `Program.cs`：`Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger(); builder.Host.UseSerilog();`

  * `appsettings.json` 增加 Serilog 节点（Console + File，最简配置）

### 阶段 1 汇总（Markdown）

```
# 阶段 1 完成项
- 创建解决方案与 4 个项目（Web/RCL/bunit/Playwright）
- 添加项目引用（Web→RCL，bunit→RCL）
- 添加核心 NuGet 包（MudBlazor 8.*、Serilog、bunit、Playwright）
- 生成 .sln 并迁移到 .slnx（可与 VS/CLI 协同）
- Web 初始集成 MudBlazor 与 Serilog（最小化配置）
```

## 阶段 2（运行与验证）

* 构建与运行 Web：

  * `dotnet build`

  * `dotnet run --project src/MudBlazorLab.Web`

  * 访问主页确认加载成功

* 安装 Playwright 浏览器（先构建一次以生成 `playwright.ps1`）：

  * `dotnet build tests/MudBlazorLab.E2E`

  * `pwsh tests/MudBlazorLab.E2E/bin/Debug/net9.0/playwright.ps1 install`

* 运行空测试项目以验证引用正常：

  * `dotnet test tests/MudBlazorLab.ComponentTests`

  * `dotnet test tests/MudBlazorLab.E2E`

### 阶段 2 汇总（Markdown）

```
# 阶段 2 完成项
- Web 可启动并正常访问
- Playwright 浏览器安装成功
- 两个测试项目可编译运行（暂为空测试）
```

## 阶段 3（示例组件 + bunit/Playwright 测试）

* 在 RCL 新增示例组件 `HelloMud.razor`（演示按钮计数与文本显示）：

```razor
@using MudBlazor
<MudText Typo="Typo.h6">计数：@count</MudText>
<MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="Inc">增加</MudButton>
@code {
  int count;
  void Inc() { count++; }
}
```

* 在 Web 新增页面 `Pages/Hello.razor` 引用组件：

```razor
@page "/hello"
<HelloMud />
```

* bunit 组件测试（xUnit）：

```csharp
using Bunit;
using MudBlazorLab.Components;
using Xunit;

public class HelloMudTests : TestContext
{
  [Fact]
  public void Click_Increments_Count()
  {
    var cut = RenderComponent<HelloMud>();
    cut.Find("button").Click();
    Assert.Contains("计数：1", cut.Markup);
  }
}
```

* Playwright 端到端测试（xUnit，读取 `BASE_URL` 环境变量）：

```csharp
using Microsoft.Playwright;
using Xunit;

public class HelloPageE2E
{
  [Fact]
  public async Task Click_Increments()
  {
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
    var page = await browser.NewPageAsync();
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    await page.GotoAsync($"{baseUrl}/hello");
    await page.GetByRole(AriaRole.Button, new() { Name = "增加" }).ClickAsync();
    await Microsoft.Playwright.Assertions.Expect(page.Locator("text=计数：1")).ToBeVisibleAsync();
  }
}
```

* 运行测试：

  * 启动 Web：`dotnet run --project src/MudBlazorLab.Web`

  * 设置环境变量：`$env:BASE_URL="http://localhost:5000"`（如端口不同按实际调整）

  * 运行组件测试：`dotnet test tests/MudBlazorLab.ComponentTests`

  * 运行 E2E 测试：`dotnet test tests/MudBlazorLab.E2E`

### 阶段 3 汇总（Markdown）

```
# 阶段 3 完成项
- RCL 新增示例组件 `HelloMud`
- Web 暴露路由 `/hello`
- bunit 测试验证点击后计数为 1
- Playwright 测试端到端验证页面行为
```

## 阶段 4（流程总结与后续规范）

* 规范要点：

  * 组件统一在 RCL 维护，页面仅组合与路由

  * Serilog 使用 `appsettings.json` 管理输出与最少化格式

  * Playwright 使用环境变量传递被测地址，避免端口变化导致不稳定

  * 测试分层：bunit（纯组件）、Playwright（端到端）

* 扩展建议：

  * 在 `build/Directory.Build.props` 固定包版本与启用警告为错误

  * 引入 `Microsoft.AspNetCore.Mvc.Testing` 以自托管 Web 进行 E2E（进阶）

  * 集成 CI（GitHub Actions）执行 `dotnet build/test` 与 Playwright 浏览器安装

### 阶段 4 汇总（Markdown）

```
# 阶段 4 完成项
- 明确组件放置、日志、测试分层规范
- 给出可扩展点与 CI 方向
```

## 参考

* .slnx CLI 支持（.NET SDK 9.0.200，VS 17.14）：<https://devblogs.microsoft.com/dotnet/introducing-slnx-support-dotnet-cli/>

* Playwright .NET 安装与浏览器：

  * <https://playwright.dev/dotnet/docs/intro>

  * <https://playwright.dev/dotnet/docs/browsers>

* bunit（.NET 8+ 兼容）：

  * <https://bunit.dev/docs/getting-started/create-test-project.html>

  * <https://www.nuget.org/packages/bunit/>

* MudBlazor 8.\*（支持 .NET 8/9）：

  * <https://www.nuget.org/packages/MudBlazor/8.0.0>

  * <https://www.nuget.org/packages/MudBlazor/latest>

