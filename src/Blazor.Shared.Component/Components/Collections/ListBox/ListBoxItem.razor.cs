using System.Collections.ObjectModel;
using Blazor.Shared.Components;
using Blazor.Shared.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Blazor.Shared.Components;

public partial class ListBoxItem : StyledComponentBase
{
    private readonly ICollection<ContextMenuItem> _contextMenuItems = new ObservableCollection<ContextMenuItem>();

    [Parameter]
    public object Item { get; set; } = default!;

    [Parameter]
    public bool IsSelected { get; set; }

    [Parameter]
    public EventCallback<object> OnSelected { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public EventCallback<ListBoxItemBuildingContextMenuEventArgs> OnBuildingContextMenu { get; set; }

    protected override void OnParametersSet()
    {
        if (IsSelected)
        {
            CSS.Add("selected");
        }
        else
        {
            CSS.Remove("selected");
        }

        if (IsActuallyEnabled)
        {
            CSS.Remove("disabled");
        }
        else
        {
            CSS.Add("disabled");
        }

        base.OnParametersSet();
    }

    private Task OnClickAsync()
    {
        return OnSelected.InvokeAsync(Item);
    }

    private Task OnContextMenuOpeningAsync()
    {
        return OnBuildingContextMenu.InvokeAsync(new ListBoxItemBuildingContextMenuEventArgs(Item, _contextMenuItems));
    }

    private async Task OnKeyDownAsync(KeyboardEventArgs ev)
    {
        if (string.Equals(ev.Code, "Enter", StringComparison.OrdinalIgnoreCase)
            || string.Equals(ev.Code, "Space", StringComparison.OrdinalIgnoreCase))
        {
            await OnClickAsync();
        }
    }
}
