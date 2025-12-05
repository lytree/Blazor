


using CommunityToolkit.Diagnostics;

namespace Blazor.Hybrid.Core;
/// <summary>
/// Represents the factory to access some resources of the current assembly such as strings stored in RESX files, or Fonts.
/// </summary>
/// <remarks>
/// <example>
///     <code>
///         [Export(typeof(MyResourceAssemblyIdentifier))]
///         [Name(nameof(MyResourceAssemblyIdentifier))]
///         internal sealed class MyResourceAssemblyIdentifier : IResourceAssemblyIdentifier
///         {
///         }
///     </code>
/// </example>
/// </remarks>
public interface IResourceAssemblyIdentifier
{
    /// <summary>
    /// Get access to fonts that can be used by <see cref="IGuiTool"/>.
    /// </summary>
    /// <remarks>
    /// The font is expected to be a TTF or OTF. WOFF and WOFF2 aren't supported at the moment.
    /// </remarks>
    /// <returns>An array of font definition with a stream to access it.</returns>
    ValueTask<FontDefinition[]> GetFontDefinitionsAsync();
    /// <summary>
    /// Get access to css that can be used by <see cref="IGuiTool"/>.
    /// </summary>
    /// <returns></returns>
    ValueTask<string[]> GetCssDefinitionsAsync();
}

/// <summary>
/// Represents a font definition.
/// </summary>
public sealed class FontDefinition : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FontDefinition"/> class.
    /// </summary>
    /// <param name="fontFamily">The font family.</param>
    /// <param name="fontReader">The stream to read the font file.</param>
    public FontDefinition(string fontFamily, Stream fontReader)
    {
        Guard.IsNotNullOrWhiteSpace(fontFamily);
        Guard.IsNotNull(fontReader);
        Guard.CanRead(fontReader);
        FontFamily = fontFamily;
        FontReader = fontReader;
    }

    /// <summary>
    /// Gets the font family.
    /// </summary>
    public string FontFamily { get; }

    /// <summary>
    /// Gets the stream to read the font file.
    /// </summary>
    public Stream FontReader { get; }

    /// <summary>
    /// Disposes the font reader.
    /// </summary>
    public void Dispose()
    {
        FontReader?.Dispose();
    }
}
