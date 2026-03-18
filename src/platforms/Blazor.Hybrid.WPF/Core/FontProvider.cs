using System.Windows.Media;
using Blazor.Hybrid.Shared;
using Microsoft.Extensions.Logging;

namespace Blazor.Hybrid.Windows.Core;

internal sealed partial class FontProvider : IFontProvider
{
    private readonly Lazy<string[]> _fontFamilies
        = new(() => Fonts.SystemFontFamilies.SelectMany(f => f.FamilyNames.Values).Order().ToArray());

    private readonly ILogger _logger;


    public FontProvider(ILogger<IFontProvider> logger)
    {
        _logger = logger;
    }

    public string[] GetFontFamilies()
    {
        try
        {
            return _fontFamilies.Value;
        }
        catch (Exception ex)
        {
            LogGetFontFamiliesFailed(ex);
        }

        return Array.Empty<string>();
    }

    [LoggerMessage(1, LogLevel.Error, "Failed to retrieve the list of fonts installed on the system.")]
    partial void LogGetFontFamiliesFailed(Exception ex);
}
