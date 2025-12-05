


using Blazor.Localization.Strings.ToolPage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blazor.Shared.Components;


[Export]
[PartCreationPolicy(CreationPolicy.NonShared)]
internal sealed partial class ToolPageViewModel : ObservableRecipient
{

    private GuiToolViewItem? _guiToolViewItem;

    [ObservableProperty]
    private string _headerText = string.Empty;

    /// <summary>
    /// Indicates whether the <see cref="SelectedMenuItem"/> can be added or removed from favorites.
    /// </summary>
    internal bool IsSelectedMenuItemSupportFavorite => _guiToolViewItem is not null && !_guiToolViewItem.ToolInstance.NotFavorable;

    /// <summary>
    /// Indicates whether the <see cref="SelectedMenuItem"/> is a favorite tool or not.
    /// </summary>
    // internal bool IsSelectedMenuItemAFavoriteTool => _guiToolViewItem is not null && _guiToolProvider.GetToolIsFavorite(_guiToolViewItem.ToolInstance);

    /// <summary>
    /// Gets the UI of the tool.
    /// </summary>
    internal UIToolView? ToolView
    {
        get
        {
            Guard.IsNotNull(_guiToolViewItem);
            return _guiToolViewItem.ToolInstance.View;
        }
    }

    internal void Load(GuiToolViewItem guiToolViewItem)
    {
        Guard.IsNotNull(guiToolViewItem);

        // if (_guiToolViewItem != guiToolViewItem)
        // {
        //     // Add the tool to most recent ones.
        //     _guiToolProvider.SetMostRecentUsedTool(guiToolViewItem.ToolInstance);

        //     _guiToolViewItem = guiToolViewItem;

        //     HeaderText = guiToolViewItem.ToolInstance.LongOrShortDisplayTitle;
        // }
    }

    /// <summary>
    /// Toggles the favorite status of the <see cref="SelectedMenuItem"/>.
    /// </summary>
    [RelayCommand]
    private void ToggleSelectedMenuItemFavorite()
    {
        // Guard.IsNotNull(_guiToolViewItem);
        // _guiToolProvider.SetToolIsFavorite(_guiToolViewItem.ToolInstance, !_guiToolProvider.GetToolIsFavorite(_guiToolViewItem.ToolInstance));
        // OnPropertyChanged(nameof(IsSelectedMenuItemAFavoriteTool));
    }

    /// <summary>
    /// Rebuild the view of the tool.
    /// </summary>
    [RelayCommand]
    private void RebuildView()
    {
        Guard.IsNotNull(_guiToolViewItem);
        _guiToolViewItem.ToolInstance.RebuildView();
        OnPropertyChanged(nameof(ToolView));
    }

    internal string GetFavoriteButtonText(bool isSelectedMenuItemAFavoriteTool)
    {
        Guard.IsNotNull(_guiToolViewItem);
        if (isSelectedMenuItemAFavoriteTool)
        {
            return ToolPage.RemoveFromFavorites;
        }

        return ToolPage.AddToFavorites;
    }
}
