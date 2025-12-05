using System.Diagnostics;
using CommunityToolkit.Diagnostics;

namespace Blazor.Hybrid.Core.Metadata;

[DebuggerDisplay($"InternalComponentName = {{{nameof(InternalComponentName)}}}")]
public sealed class LanguageServiceMetadata
{
    public string InternalComponentName { get; }

    public LanguageServiceMetadata(IDictionary<string, object> metadata)
    {
        InternalComponentName = metadata.GetValueOrDefault(nameof(NameAttribute.InternalComponentName)) as string ?? string.Empty;
        Guard.IsNotNullOrWhiteSpace(InternalComponentName);
    }
}
