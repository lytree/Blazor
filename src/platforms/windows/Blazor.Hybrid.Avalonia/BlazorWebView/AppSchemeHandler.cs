using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xilium.CefGlue;

namespace Blazor.Hybrid.Avalonia;
internal class AppSchemeHandler : CefSchemeHandlerFactory
{
    protected override CefResourceHandler Create(CefBrowser browser, CefFrame frame, string schemeName, CefRequest request)
    {
        // todo
        return null;
    }
}
