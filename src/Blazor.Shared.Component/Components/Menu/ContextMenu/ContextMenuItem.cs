using Blazor.Shared.Components;

namespace Blazor.Shared.Layout;

public sealed class ContextMenuItem : DropDownListItem
{
    internal string? KeyboardShortcut { get; set; } // TODO: Actually handle keyboard shortcuts?
}
