using System.Text.Encodings.Web;
using System.Threading.Channels;
using Blazor.Hybrid.Avalonia;
using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace Blazor.Hybrid.Avalonia;

/// <summary>
/// An implementation of <see cref="WebViewManager"/> that uses the <see cref="WKWebView"/> browser control
/// to render web content.
/// </summary>
public partial class BlazorWebViewManager : WebViewManager
{
    private readonly BlazorWebView _blazorMauiWebViewHandler;
    private readonly ILogger _logger;
    private readonly string _contentRootRelativeToAppRoot;
    private readonly Channel<string> _channel;

    /// <summary>
    /// Initializes a new instance of <see cref="WebViewManager"/>
    /// </summary>
    /// <param name="blazorMauiWebViewHandler">The <see cref="BlazorWkWebView"/>.</param>
    /// <param name="provider">The <see cref="IServiceProvider"/> for the application.</param>
    /// <param name="fileProvider">Provides static content to the webview.</param>
    /// <param name="jsComponents">Describes configuration for adding, removing, and updating root components from JavaScript code.</param>
    /// <param name="contentRootRelativeToAppRoot">Path to the directory containing application content files.</param>
    /// <param name="hostPageRelativePath">Path to the host page within the fileProvider.</param>
    internal BlazorWebViewManager(
        Uri baseUri,
        BlazorWebView blazorMauiWebViewHandler,
        IServiceProvider provider,
        IFileProvider fileProvider,
        JSComponentConfigurationStore jsComponents,
        string contentRootRelativeToAppRoot,
        string hostPageRelativePath)
        : base(
            provider,
            Dispatcher.CreateDefault(),
            baseUri,
            fileProvider,
            jsComponents,
            hostPageRelativePath)
    {
        Guard.IsNotNull(blazorMauiWebViewHandler);

        _logger = NullLogger.Instance;
        _blazorMauiWebViewHandler = blazorMauiWebViewHandler;
        _contentRootRelativeToAppRoot = contentRootRelativeToAppRoot;

        // https://github.com/DevToys-app/DevToys/issues/1194
        // Forked from https://github.com/tryphotino/photino.Blazor/issues/40
        //Create channel and start reader
        _channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });
        Task.Run(SendMessagePump);
    }

    internal bool TryGetResponseContentInternal(
        string uri,
        bool allowFallbackOnHostPage,
        out int statusCode,
        out string statusMessage,
        out Stream content,
        out IDictionary<string, string> headers)
    {
        bool defaultResult
            = TryGetResponseContent(
                uri,
                allowFallbackOnHostPage,
                out statusCode,
                out statusMessage,
                out content,
                out headers);
        bool hotReloadedResult
            = StaticContentHotReloadManager.TryReplaceResponseContent(
                _contentRootRelativeToAppRoot,
                uri,
                ref statusCode,
                ref content,
                headers);
        return defaultResult || hotReloadedResult;
    }

    internal void MessageReceivedInternal(Uri uri, string message)
    {
        MessageReceived(uri, message);
    }

    /// <inheritdoc />
    protected override ValueTask DisposeAsyncCore()
    {
        // Complete channel
        try
        {
            _channel.Writer.Complete();
        }
        catch
        {
            // Ignore.
        }

        // Continue disposing
        return base.DisposeAsyncCore();
    }

    /// <inheritdoc />
    protected override void NavigateCore(Uri absoluteUri)
    {
        LogNavigatingToUri(absoluteUri);
    }

    /// <inheritdoc />
    protected override void SendMessage(string message)
    {
        string messageJsStringLiteral = JavaScriptEncoder.Default.Encode(message);
        string script = $"__dispatchMessageCallback(\"{messageJsStringLiteral}\")";

        // https://github.com/DevToys-app/DevToys/issues/1194
        // Forked from https://github.com/tryphotino/photino.Blazor/issues/40
        while (!_channel.Writer.TryWrite(script))
        {
            Thread.Sleep(200);
        }
    }

    private async Task SendMessagePump()
    {
        // https://github.com/DevToys-app/DevToys/issues/1194
        // Forked from https://github.com/tryphotino/photino.Blazor/issues/40
        ChannelReader<string> reader = _channel.Reader;
        try
        {
            while (true)
            {
                string script = await reader.ReadAsync();
               
            }
        }
        catch (ChannelClosedException) { }
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Navigating to {uri}.")]
    partial void LogNavigatingToUri(Uri uri);
}
