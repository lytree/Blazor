using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xilium.CefGlue;
using Xilium.CefGlue.Common.Handlers;

namespace Blazor.Hybrid.Avalonia;
internal class AppResourceHandler : DefaultResourceHandler
{

    protected override RequestHandlingFashion ProcessRequestAsync(CefRequest request, CefCallback callback)
    {

        Console.WriteLine(request);  
        return RequestHandlingFashion.ContinueAsync;
    }

    protected override bool Read(Stream outResponse, int bytesToRead, out int bytesRead, CefResourceReadCallback callback)
    {
        var buffer = new byte[bytesToRead];
        bytesRead = Response?.Read(buffer, 0, buffer.Length) ?? 0;

        if (bytesRead == 0)
        {
            return false;
        }

        outResponse.Write(buffer, 0, bytesRead);
        return bytesRead > 0;
    }
}
