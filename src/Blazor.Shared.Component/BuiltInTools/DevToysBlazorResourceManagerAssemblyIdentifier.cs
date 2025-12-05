using System.Reflection;
using Blazor.Hybrid.Core;

namespace Blazor.Shared.BuiltInTools;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(DevToysBlazorResourceManagerAssemblyIdentifier))]
public sealed class DevToysBlazorResourceManagerAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        string fluentSystemIconsResourceName = "Blazor.Shared.Assets.fonts.FluentSystemIcons-Regular.ttf";
        string devToysToolsIconsResourceName = "Blazor.Shared.Assets.fonts.DevToys-Tools-Icons.ttf";

        Stream fluentSystemIconsResourceStream = assembly.GetManifestResourceStream(fluentSystemIconsResourceName)!;
        Stream devToysToolsIconsResourceStream = assembly.GetManifestResourceStream(devToysToolsIconsResourceName)!;
        return new ValueTask<FontDefinition[]>(
        [
            new FontDefinition("FluentSystemIcons", fluentSystemIconsResourceStream),
            new FontDefinition("DevToys-Tools-Icons", devToysToolsIconsResourceStream)
        ]);
    }
    public ValueTask<string[]> GetCssDefinitionsAsync()
    {
        throw new NotImplementedException();
    }
}
