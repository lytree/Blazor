using System.IO;
using Blazor.Hybrid.Core;

namespace Blazor.Hybrid.Windows.Core;

internal static class Constants
{
    internal static readonly string AppCacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppHelper.IsPreviewVersion.Value ? "DevToys-preview" : "DevToys");

    internal static string PluginInstallationFolder => Path.Combine(AppCacheDirectory, "Plugins");

    internal static string AppTempFolder => Path.Combine(AppCacheDirectory, "Temp");
}
