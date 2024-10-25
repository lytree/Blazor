using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xilium.CefGlue;

namespace Blazor.Hybrid.Avalonia;
internal class AppSchemeHandlerFactory : CefSchemeHandlerFactory
{
    protected override CefResourceHandler Create(CefBrowser browser, CefFrame frame, string schemeName, CefRequest request)
    {
        // todo
        if (schemeName == "app")
        {
            return new AppResourceHandler();
        }
        //if (schemeName == "http")
        //{
        //   return new HttpResourceHandler();
        //}
        return null;
    }
}
