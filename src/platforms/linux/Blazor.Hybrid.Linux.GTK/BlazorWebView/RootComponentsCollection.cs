using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components.Web;

namespace Blazor.Hybrid.Linux.GTK.BlazorWebView;

/// <summary>
/// A collection of <see cref="RootComponent"/> items.
/// </summary>
internal sealed class RootComponentsCollection : ObservableCollection<RootComponent>, IJSComponentConfiguration
{
    /// <inheritdoc />
    public JSComponentConfigurationStore JSComponents { get; } = new();
}
