
using Blazor.Shared.Core;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.FileProviders;

namespace Blazor.Hybrid.Windows.Controls.WebView;

internal sealed class CustomBlazorWebView : BlazorWebView
{
    public override IFileProvider CreateFileProvider(string contentRootDir)
    {
        var embeddedProvider = new BlazorEmbeddedFileProvider();
        var physicalProvider = new PhysicalFileProvider(contentRootDir);
        return new CompositeFileProvider(physicalProvider, embeddedProvider);
    }
}
