#:package Cake.Frosting@6.2.0
#:property TargetFramework=net10.0
#:property OutputType=Exe
#:property RunWorkingDirectory=$(MSBuildProjectDirectory)


using Cake.Common.IO;
using Cake.Core;
using Cake.Frosting;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Clean;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Common.Tools.DotNet.Restore;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<BuildContext>()
            .Run(args);
    }
}

// 定义构建中使用的常量
public static class BuildParameters
{
    public const string LinuxSolutionPath = "../Blazor-Linux.slnx";
    public const string WindowsSolutionPath = "../Blazor-Windows.slnx";
    public const string ServerSolutionPath = "../Blazor-Server.slnx";

    // 各平台入口工程。Solution 级别的构建不支持指定 RID（NETSDK1134），
    // 因此还原/编译/发布都针对入口工程执行，其 ProjectReference 会自动继承同一个 RID。
    public const string LinuxEntryProject = "../src/platforms/Blazor.Hybrid.GTK/Blazor.Hybrid.GTK.csproj";
    public const string WindowsEntryProject = "../src/platforms/Blazor.Hybrid.WPF/Blazor.Hybrid.WPF.csproj";
    public const string ServerEntryProject = "../src/platforms/Blazor.Hybrid.Server/Blazor.Hybrid.Server.csproj";

    public const string Configuration = "Release";
    public const string PackageOutputDirectory = "../packages";

    public const string LinuxRuntime = "linux-x64";
    public const string WindowsRuntime = "win-x64";

    /// <summary>可选版本号参数名，用法：--Version=1.2.3</summary>
    public const string VersionArgument = "Version";
}

public class BuildContext : FrostingContext
{
    /// <summary>通过 --Version 传入的版本号，未传入时为 null。</summary>
    public string? Version { get; }

    public bool HasVersion { get; }

    /// <summary>用于 Clean 的解决方案文件。</summary>
    public string SolutionPath { get; }

    /// <summary>还原 / 编译 / 发布的入口工程。</summary>
    public string EntryProject { get; }

    /// <summary>当前目标对应的运行时标识（RID）。Server 目标同时包含 win-x64 与 linux-x64。</summary>
    public string[] RuntimeIdentifiers { get; }

    /// <summary>各 RID 对应的发布输出目录。</summary>
    public string OutputDirectoryFor(string runtime)
        => $"{BuildParameters.PackageOutputDirectory}/{runtime}";

    public BuildContext(ICakeContext context)
        : base(context)
    {
        Version = context.Arguments.GetArgument(BuildParameters.VersionArgument);
        HasVersion = !string.IsNullOrWhiteSpace(Version);

        // 获取当前目标任务
        var target = context.Arguments.GetArgument("Target") ?? "Default";

        // 根据目标设置解决方案、入口工程与 RID
        if (target.Contains("Linux", StringComparison.OrdinalIgnoreCase))
        {
            SolutionPath = BuildParameters.LinuxSolutionPath;
            EntryProject = BuildParameters.LinuxEntryProject;
            RuntimeIdentifiers = [BuildParameters.LinuxRuntime];
            context.Log.Information("初始化：Linux 环境");
        }
        else if (target.Contains("Windows", StringComparison.OrdinalIgnoreCase))
        {
            SolutionPath = BuildParameters.WindowsSolutionPath;
            EntryProject = BuildParameters.WindowsEntryProject;
            RuntimeIdentifiers = [BuildParameters.WindowsRuntime];
            context.Log.Information("初始化：Windows 环境");
        }
        else
        {
            SolutionPath = BuildParameters.ServerSolutionPath;
            EntryProject = BuildParameters.ServerEntryProject;
            RuntimeIdentifiers = [BuildParameters.WindowsRuntime, BuildParameters.LinuxRuntime];
            context.Log.Information("初始化：Server环境（win-x64 与 linux-x64）");
        }
    }
}

// 统一的构建/发布参数：还原、编译、发布使用完全一致的 RID 与自包含设置，
// 这样 Publish 才能用 NoBuild/NoRestore 跳过重复编译。
internal static class Settings
{
    public static DotNetRestoreSettings Restore(string runtime)
        => new()
        {
            Runtime = runtime,
            ArgumentCustomization = args => args.Append("/p:SelfContained=true")
        };

    public static DotNetBuildSettings Build(BuildContext context, string runtime)
        => new()
        {
            Configuration = BuildParameters.Configuration,
            Runtime = runtime,
            NoRestore = true,
            ArgumentCustomization = args =>
            {
                args.Append("/p:SelfContained=true");
                return AppendCommonArguments(context, args);
            }
        };

    public static DotNetPublishSettings Publish(BuildContext context, string runtime, string outputDirectory)
        => new()
        {
            Configuration = BuildParameters.Configuration,
            Runtime = runtime,
            SelfContained = true,
            NoBuild = true,
            NoRestore = true,
            OutputDirectory = outputDirectory,
            ArgumentCustomization = args => AppendCommonArguments(context, args)
        };

    private static ProcessArgumentBuilder AppendCommonArguments(BuildContext context, ProcessArgumentBuilder args)
    {
        args.Append("/property:GenerateFullPaths=true");
        args.Append("/consoleloggerparameters:NoSummary");

        if (context.HasVersion)
        {
            args.Append($"/p:Version={context.Version}");
        }

        return args;
    }
}

[TaskName("Clean")]
public sealed class CleanTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        // 清理解决方案
        context.DotNetClean(context.SolutionPath, new DotNetCleanSettings
        {
            Configuration = BuildParameters.Configuration
        });
        // 清理发布输出目录
        context.CleanDirectory(BuildParameters.PackageOutputDirectory);
    }
}

[TaskName("Restore")]
[IsDependentOn(typeof(CleanTask))]
public sealed class RestoreTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        // 按 RID 还原入口工程，确保 project.assets.json 中存在 net10.0/<rid> 目标
        foreach (var runtime in context.RuntimeIdentifiers)
        {
            context.Log.Information($"还原 {context.EntryProject}（{runtime}）");
            context.DotNetRestore(context.EntryProject, Settings.Restore(runtime));
        }
    }
}

[TaskName("Compile")]
[IsDependentOn(typeof(RestoreTask))]
public class CompileTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        // 使用与 Publish 相同的 RID 编译入口工程（ProjectReference 继承同一 RID），
        // Publish 阶段即可用 NoBuild 跳过重复编译。
        foreach (var runtime in context.RuntimeIdentifiers)
        {
            context.Log.Information($"编译 {context.EntryProject}（{runtime}）");
            context.DotNetBuild(context.EntryProject, Settings.Build(context, runtime));
        }
    }
}

[TaskName("Publish-Linux")]
[IsDependentOn(typeof(CompileTask))]
public sealed class PublishLinuxTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetPublish(
            BuildParameters.LinuxEntryProject,
            Settings.Publish(context, BuildParameters.LinuxRuntime, context.OutputDirectoryFor(BuildParameters.LinuxRuntime)));
    }
}

[TaskName("Publish-Windows")]
[IsDependentOn(typeof(CompileTask))]
public sealed class PublishWindowsTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetPublish(
            BuildParameters.WindowsEntryProject,
            Settings.Publish(context, BuildParameters.WindowsRuntime, context.OutputDirectoryFor(BuildParameters.WindowsRuntime)));
    }
}

[TaskName("Publish-Server")]
[IsDependentOn(typeof(CompileTask))]
public sealed class PublishServerTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        foreach (var runtime in context.RuntimeIdentifiers)
        {
            context.DotNetPublish(
                BuildParameters.ServerEntryProject,
                Settings.Publish(context, runtime, context.OutputDirectoryFor(runtime)));
        }
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(PublishServerTask))]
public class DefaultTask : FrostingTask
{
}
