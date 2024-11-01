using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xilium.CefGlue;
using Xilium.CefGlue.Common.Handlers;

namespace Blazor.Hybrid.Avalonia;

internal class HttpResourceHandler : DefaultResourceHandler
{

    private const string AccessControlAllowOriginHeaderKey = "Access-Control-Allow-Origin";

    internal static readonly CefResourceType[] AcceptedResources = new CefResourceType[] {
        // These resources types need an "Access-Control-Allow-Origin" header response entry
        // to comply with CORS security restrictions.
        CefResourceType.SubFrame,
        CefResourceType.FontResource,
        CefResourceType.Stylesheet
    };

    protected override RequestHandlingFashion ProcessRequestAsync(CefRequest request, CefCallback callback)
    {
        Task.Run(async () =>
        {

            using (HttpClient client = new())
            {
                // 添加请求头
                foreach (var header in Headers.AllKeys)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(header, Headers.GetValues(header));
                }
                try
                {
                    HttpResponseMessage response = await client.GetAsync(request.Url);

                    response.EnsureSuccessStatusCode(); // 如果响应不是成功状态，则抛出异常
                                                        // 获取响应流
                    Response = await response.Content.ReadAsStreamAsync();
                    // 获取 MIME 类型
                    MimeType = response.Content.Headers.ContentType?.ToString();
                    // 获取状态码
                    Status = (int)response.StatusCode;

                    // 获取状态描述
                    StatusText = response.ReasonPhrase;

                    // 获取响应头
                    var responseHeaders = response.Headers;
                    // 移除并添加 CORS 头
                    response.Headers.Remove(AccessControlAllowOriginHeaderKey);
                    response.Headers.TryAddWithoutValidation(AccessControlAllowOriginHeaderKey, "*");
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"\n请求错误: {e.Message}");
                }
                finally
                {
                    callback.Continue();
                }
            }

        });
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
