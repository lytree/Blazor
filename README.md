# Blazor Hybrid

一套 Razor 组件（`Blazor.Component.Shared`），通过 Blazor Hybrid / Blazor Server 宿主同时运行在三个平台上：

| 平台 | 宿主工程 | 目标框架 | 技术栈 |
|---|---|---|---|
| Windows 桌面 | `src/platforms/Blazor.Hybrid.WPF` | `net10.0-windows10.0.17763.0` | WPF + `Microsoft.AspNetCore.Components.WebView.Wpf` + WinForms（剪贴板） |
| Linux 桌面 | `src/platforms/Blazor.Hybrid.GTK` | `net10.0` | GirCore `Gtk-4.0` / `WebKit-6.0` + 自实现 BlazorWebView |
| Web / Server | `src/platforms/Blazor.Hybrid.Server` | `net10.0` | ASP.NET Core Razor Components（Interactive Server） |

## 目录结构

```
├─ Blazor-Windows.slnx         # Windows 平台解决方案
├─ Blazor-Linux.slnx           # Linux 平台解决方案
├─ Blazor-Server.slnx          # Server 平台解决方案
├─ build/Build.cs              # Cake.Frosting 构建脚本（文件式应用）
├─ build.ps1 / build.sh        # 构建入口包装脚本
├─ global.json                 # 固定 SDK 版本
├─ src/
│  ├─ Directory.Build.props    # 全局项目属性（Nullable / LangVersion 等）
│  ├─ Directory.Build.targets  # 全局 MSBuild 目标（Windows 长路径校验）
│  ├─ Directory.Packages.props # 中央包管理（CPM）版本清单
│  ├─ Environment.props        # 平台常量（WINDOWS / LINUX / MAC ...）
│  ├─ Blazor.Component.Shared/ # 共享 Razor 组件库（RCL）
│  │  ├─ Core/                 # 设置定义、嵌入式文件提供程序等
│  │  ├─ Layout/ Pages/ Theme/ Threading/
│  │  └─ wwwroot/
│  └─ platforms/
│     ├─ Blazor.Hybrid.Shared/ # 跨平台共享的宿主层抽象（Device / Interface）
│     ├─ Blazor.Hybrid.WPF/    # Windows 宿主（Core / Native / Controls / Strings）
│     ├─ Blazor.Hybrid.GTK/    # Linux 宿主（BlazorWebView 实现 / Core / Strings）
│     └─ Blazor.Hybrid.Server/ # Server 宿主
└─ packages/                   # 发布产物输出目录（构建生成，不入库）
```

## 环境要求

- **.NET SDK 10.0.401**（由 `global.json` 固定，`rollForward: latestFeature`，不允许预览版）
- **Windows**：Windows 10 17763 及以上；建议开启长路径支持（`Directory.Build.targets` 会校验 `LongPathsEnabled`）：
  ```powershell
  Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem' -Name LongPathsEnabled -Value 1
  ```
- **Linux**：Ubuntu 22.04 及以上，需安装原生依赖：
  ```bash
  sudo apt install libgtk-4-dev libadwaita-1-0 libwebkitgtk-6.0-4
  ```

## 构建与运行

直接使用对应平台的解决方案：

```powershell
dotnet build Blazor-Server.slnx    -c Release
dotnet build Blazor-Windows.slnx   -c Release

# 运行
dotnet run --project src/platforms/Blazor.Hybrid.Server
dotnet run --project src/platforms/Blazor.Hybrid.WPF
dotnet run --project src/platforms/Blazor.Hybrid.GTK     # 需 Linux 环境
```

使用 Cake 构建脚本（`build/Build.cs`）：

```powershell
.\build.ps1 --target=Compile          # Clean -> Restore -> Compile
.\build.ps1 --target=Publish-Windows  # 发布 WPF 到 packages/win-x64
.\build.ps1 --target=Publish-Linux    # 发布 GTK 到 packages/linux-x64
.\build.ps1 --target=Publish-Server   # 发布 Server 到 packages/{win,linux}-x64
```

> **注意**：`Compile` / `Restore` / `Clean` 任务会依据目标名是否包含 `Windows` / `Linux` 自动选择解决方案，其余情况一律使用 **Server 解决方案**。
>
> 发布产物为自包含（self-contained）部署，输出位于 `packages/` 目录。

## 依赖管理

项目启用 **中央包管理（CPM）**：所有包版本集中在 `src/Directory.Packages.props`，各 `csproj` 中只写 `<PackageReference Include="..." />`，不写版本号；同时开启了传递依赖固定（`CentralPackageTransitivePinningEnabled`）。新增依赖时请同步更新版本清单。

## 开发约定

- C# 语言版本 14，启用 `Nullable` 与 `ImplicitUsings`（见 `Directory.Build.props`）
- 代码风格由根目录 `.editorconfig` 统一约束（文件作用域命名空间、`System.*` using 优先、4 空格缩进）
- 行尾由 `.gitattributes`（`* text=auto`）统一归一化，`.cs` 的 `end_of_line` 交由 git 处理
- 平台差异通过 `Environment.props` 定义的 `WINDOWS` / `LINUX` 等编译常量区分

## 已知事项

- 证书文件不入库（`.gitignore` 忽略 `*.pfx`）。Server 项目本地开发如需 HTTPS，请使用 `dotnet dev-certs https` 生成开发证书。
- GTK 端的 `BlazorWebView` 为自实现版本（非官方包），与上游 `Microsoft.AspNetCore.Components.WebView` 的互操作脚本存在耦合，升级 WebView 相关包时需重点回归。
- 三平台各自维护一份 `bootstrap.min.css`，如需统一可考虑迁移至 `Blazor.Component.Shared`，由 RCL 静态资源机制统一分发。
