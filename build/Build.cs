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
using Cake.Core.Diagnostics;
using System.Text.Json;

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
    public const string LinuxSolutionPath = "../Blazor-Linux.slnx"; // 确保路径正确
    public const string WindowsSolutionPath = "../Blazor-Windows.slnx"; // 确保路径正确
    public const string ServerSolutionPath = "../Blazor-Server.slnx"; // 确保路径正确
    public const string Configuration = "Release";
    public const string PackageOutputDirectory = "../packages";
    public const string Version = "Version";
}
public class BuildContext : FrostingContext
{
    public bool HasVersion { get; set; }
    public string SolutionPath { get; set; }
    public BuildContext(ICakeContext context)
        : base(context)
    {
        HasVersion = context.Arguments.HasArgument("Version");
        // 获取当前目标任务
        var target = context.Arguments.GetArgument("Target") ?? "Default";

        // 根据目标设置路径
        if (target.Contains("Linux", StringComparison.OrdinalIgnoreCase))
        {
            SolutionPath = BuildParameters.LinuxSolutionPath;
            context.Log.Information("初始化：Linux 环境");
        }
        else if (target.Contains("Windows", StringComparison.OrdinalIgnoreCase))
        {
            SolutionPath = BuildParameters.WindowsSolutionPath;
            context.Log.Information("初始化：Windows 环境");
        }
        else
        {
            SolutionPath = BuildParameters.ServerSolutionPath;
            context.Log.Information("初始化：Server环境");
        }
    }
}

[TaskName("Clean")]
public sealed class CleanTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        // 使用 DotNetCoreClean 清理 Solution
        context.DotNetClean(context.SolutionPath, new DotNetCleanSettings
        {
            Configuration = BuildParameters.Configuration
        });
        // 可选：清理 NuGet 包输出目录
        context.CleanDirectory(BuildParameters.PackageOutputDirectory);
    }
}
[TaskName("Restore")]
[IsDependentOn(typeof(CleanTask))]
public sealed class RestoreTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetRestore(context.SolutionPath);
    }
}

[TaskName("Compile")]
[IsDependentOn(typeof(RestoreTask))]
public class CompileTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetBuild(context.SolutionPath, new DotNetBuildSettings
        {
            ArgumentCustomization = args => args
                .Append("/property:GenerateFullPaths=true")
                .Append("/consoleloggerparameters:NoSummary"),
            Configuration = BuildParameters.Configuration,
            NoRestore = true // 已经在 Restore Task 中完成

        });
    }
}

[TaskName("Publish-Linux")]
[IsDependentOn(typeof(CompileTask))]
public sealed class PublishLinuxTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var outputDir = $"{BuildParameters.PackageOutputDirectory}/linux-x64";
        context.DotNetPublish(@"../src/platforms/Blazor.Hybrid.GTK/Blazor.Hybrid.GTK.csproj", new DotNetPublishSettings
        {
            Configuration = BuildParameters.Configuration,
            Runtime = "linux-x64",
            SelfContained = true,
            OutputDirectory = outputDir
        });
    }
}
[TaskName("Publish-Windows")]
[IsDependentOn(typeof(CompileTask))]
public sealed class PublishWindowsTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var outputDir = $"{BuildParameters.PackageOutputDirectory}/win-x64";
        context.DotNetPublish(@"../src/platforms/Blazor.Hybrid.WPF/Blazor.Hybrid.WPF.csproj", new DotNetPublishSettings
        {
            Configuration = BuildParameters.Configuration,
            Runtime = "win-x64",
            SelfContained = true,
            OutputDirectory = outputDir
        });
    }
}

[TaskName("Publish-Server")]
[IsDependentOn(typeof(CompileTask))]
public sealed class PublishServerTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetPublish(@"../src/platforms/Blazor.Hybrid.Server/Blazor.Hybrid.Server.csproj", new DotNetPublishSettings
        {
            Configuration = BuildParameters.Configuration,
            Runtime = "win-x64",
            SelfContained = true,
            OutputDirectory = $"{BuildParameters.PackageOutputDirectory}/win-x64"
        });
        context.DotNetPublish(@"../src/platforms/Blazor.Hybrid.Server/Blazor.Hybrid.Server.csproj", new DotNetPublishSettings
        {
            Configuration = BuildParameters.Configuration,
            Runtime = "linux-x64",
            SelfContained = true,
            OutputDirectory = $"{BuildParameters.PackageOutputDirectory}/linux-x64"
        });
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(PublishServerTask))]
public class DefaultTask : FrostingTask
{
}
