using Blazor.Shared.Layout;

namespace Blazor.Shared.Components;

public sealed class ListBoxItemBuildingContextMenuEventArgs : EventArgs
{
    internal ListBoxItemBuildingContextMenuEventArgs(object? item, ICollection<ContextMenuItem> items)
    {
        Guard.IsNotNull(items);
        ItemValue = item;
        ContextMenuItems = items;
    }

    internal object? ItemValue { get; }

    internal ICollection<ContextMenuItem> ContextMenuItems { get; }
}
