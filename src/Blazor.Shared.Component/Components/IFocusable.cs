namespace Blazor.Shared.Components;

internal interface IFocusable
{
    ValueTask<bool> FocusAsync();
}
