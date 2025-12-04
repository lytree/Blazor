using Microsoft.AspNetCore.Components;

namespace Blazor.Shared.Components;

public class DropDownListItem
{
    internal string? Text { get; set; }

    internal int IconGlyph { get; set; }

    internal string IconFontFamily { get; set; } = "FluentSystemIcons";

    internal bool IsEnabled { get; set; } = true;

    internal EventCallback<DropDownListItem> OnClick { get; set; }
}
