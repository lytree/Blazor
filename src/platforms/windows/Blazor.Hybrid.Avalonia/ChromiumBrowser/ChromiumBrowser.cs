using Xilium.CefGlue;
using Xilium.CefGlue.Avalonia;

namespace Blazor.Hybrid.Avalonia;

internal partial class ChromiumBrowser : AvaloniaCefBrowser
{
    public new void CreateBrowser(int width, int height)
    {
        if (IsBrowserInitialized)
        {
            return;
        }
        base.CreateBrowser(width, height);
    }

    internal CefBrowser GetBrowser() => UnderlyingBrowser;
}
