using NuGet.Packaging;

namespace Blazor.Shared.BuiltInTools.ExtensionsManager;

internal record ExtensionInstallationResult(bool AlreadyInstalled, NuspecReader NuspecReader, string ExtensionInstallationPath)
{
}
