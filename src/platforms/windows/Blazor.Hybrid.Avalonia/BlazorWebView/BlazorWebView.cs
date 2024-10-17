using System.Collections.Specialized;
using System.Reflection;
using Blazor.Shared.Core;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using WebViewControl;
using Exception = System.Exception;
using Task = System.Threading.Tasks.Task;
using Uri = System.Uri;

namespace Blazor.Hybrid.Avalonia.Blazor;

internal sealed partial class BlazorWebView : BaseControl, IDisposable
{


    private const string DevToysInteropName = "devtoyswebinterop";
    private const string Scheme = "app";
    internal const string AppHostAddress = "0.0.0.0";
    internal static readonly Uri AppOriginUri = new($"{Scheme}://{AppHostAddress}/");

    private const string BlazorInitScript
        = $$"""
            window.__receiveMessageCallbacks = [];
            window.__dispatchMessageCallback = function(message) {
                window.__receiveMessageCallbacks.forEach(
                    function(callback)
                    {
                        try
                        {
                            callback(message);
                        }
                        catch { }
                    });
            };
            window.external = {
                sendMessage: function(message) {
                    window.webkit.messageHandlers.{{DevToysInteropName}}.postMessage(message);
                },
                receiveMessage: function(callback) {
                    window.__receiveMessageCallbacks.push(callback);
                }
            };

            try
            {
                Blazor.start(); // It might have already started.
            }
            catch {}

            (function () {
                window.onpageshow = function(event) {
                    if (event.persisted) {
                        window.location.reload();
                    }
                };
            })();
            """;
    private readonly bool _enabledDeveloperTools;
    private readonly ILogger _logger;
    private readonly IServiceProvider _services;
    private BlazorWebViewManager? _webViewManager;
    private string? _hostPage;

    static BlazorWebView()
    {

    }

    internal BlazorWebView(IServiceProvider serviceProvider, bool enableDeveloperTools)
    {
        Guard.IsNotNull(serviceProvider);
        _services = serviceProvider;


        _enabledDeveloperTools = enableDeveloperTools;
        View = CreateWebView();

        //View.ContextMenu 
        RootComponents.CollectionChanged += RootComponentsOnCollectionChanged;
    }

    /// <summary>
    /// Gets the view corresponding to the current Blazor web view.
    /// </summary>
    internal WebView View { get; }

    /// <summary>
    /// Path to the host page within the application's static files. For example, <code>wwwroot\index.html</code>.
    /// This property must be set to a valid value for the Razor components to start.
    /// </summary>
    internal string? HostPage
    {
        get => _hostPage;
        set
        {
            _hostPage = value;
            StartWebViewCoreIfPossible();
        }
    }
    /// <summary>
    /// Gets or sets an <see cref="IServiceProvider"/> containing services to be used by this control and also by application code.
    /// This property must be set to a valid value for the Razor components to start.
    /// </summary>
    public IServiceProvider Services
    {
        get => _services!;
        set
        {
            _services = value;
            OnServicesPropertyChanged();
        }
    }
    /// <summary>
    /// Gets or sets the path for initial navigation within the Blazor navigation context when the Blazor component is finished loading.
    /// </summary>
    internal string StartPath { get; set; } = "/";

    /// <summary>
    /// A collection of <see cref="RootComponent"/> instances that specify the Blazor <see cref="IComponent"/> types
    /// to be used directly in the specified <see cref="HostPage"/>.
    /// </summary>
    internal RootComponentsCollection RootComponents { get; } = new();

    /// <summary>
    /// Allows customizing how links are opened.
    /// By default, opens internal links in the webview and external links in an external app.
    /// </summary>
    internal event EventHandler<UrlLoadingEventArgs>? UrlLoading;

    internal event EventHandler? BlazorWebViewInitialized;

    public void Dispose()
    {
        if (_webViewManager is not null)
        {
            // Dispose this component's contents and block on completion so that user-written disposal logic and
            // Blazor disposal logic will complete.
            _webViewManager?
                .DisposeAsync()
                .AsTask()
                .GetAwaiter()
                .GetResult();

            _webViewManager = null;
        }

        View.Dispose();
    }

    internal void OnUrlLoading(UrlLoadingEventArgs args)
    {
        Guard.IsNotNull(args);
        UrlLoading?.Invoke(this, args);
    }

    private void RootComponentsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // If we haven't initialized yet, this is a no-op
        if (_webViewManager is not null)
        {
            // Dispatch because this is going to be async, and we want to catch any errors
            _webViewManager.Dispatcher.InvokeAsync(async () =>
            {
                IEnumerable<RootComponent> newItems = e.NewItems!.Cast<RootComponent>().ToArray();
                IEnumerable<RootComponent> oldItems = e.OldItems!.Cast<RootComponent>().ToArray();

                foreach (RootComponent? item in newItems.Except(oldItems))
                {
                    await item.AddToWebViewManagerAsync(_webViewManager);
                }

                foreach (RootComponent? item in oldItems.Except(newItems))
                {
                    await item.RemoveFromWebViewManagerAsync(_webViewManager);
                }
            }).Forget();
        }
    }

    private void MessageReceived(Uri uri, string message)
    {
        _webViewManager?.MessageReceivedInternal(uri, message);
    }

    private WebView CreateWebView()
    {
        var webView = new WebView();




        //// Handle messages.
        //UserContentManager.ScriptMessageReceivedSignal.Connect(
        //    userContentManager,
        //    HandleScriptMessageReceivedSignal,
        //    after: false,
        //    detail: DevToysInteropName);
        //if (!userContentManager.RegisterScriptMessageHandler(DevToysInteropName, null))
        //{
        //    throw new Exception("Could not register script message handler");
        //}

        //// Add Blazor initialization script.
        //userContentManager.AddScript(
        //    UserScript.New(
        //        BlazorInitScript,
        //        injectedFrames: UserContentInjectedFrames.AllFrames,
        //        injectionTime: UserScriptInjectionTime.End,
        //        allowList: null,
        //        blockList: null));

        //// Register a "app" url scheme to handle Blazor resources
        //webView.RegisterUriScheme(Scheme, HandleUriScheme);

        return webView;
    }

    private void StartWebViewCoreIfPossible()
    {
        if (HostPage == null || _webViewManager != null)
        {
            return;
        }

        // We assume the host page is always in the root of the content directory, because it's
        // unclear there's any other use case. We can add more options later if so.
        string contentRootDir = Path.GetDirectoryName(HostPage!) ?? string.Empty;
        string hostPageRelativePath = Path.GetRelativePath(contentRootDir, HostPage!);

        // LogCreatingFileProvider(contentRootDir, hostPageRelativePath);

        IFileProvider fileProvider = CreateFileProvider(contentRootDir);

        _webViewManager = new BlazorWebViewManager(
            AppOriginUri,
            this,
            _serviceProvider,
            fileProvider,
            RootComponents.JSComponents,
            contentRootDir,
            hostPageRelativePath);

        StaticContentHotReloadManager.AttachToWebViewManagerIfEnabled(_webViewManager);

        foreach (RootComponent rootComponent in RootComponents)
        {
            // LogAddingRootComponent(rootComponent.ComponentType?.FullName ?? string.Empty,
            //     rootComponent.Selector ?? string.Empty, rootComponent.Parameters?.Count ?? 0);

            // Since the page isn't loaded yet, this will always complete synchronously
            _ = rootComponent.AddToWebViewManagerAsync(_webViewManager);
        }

        // LogStartingInitialNavigation(StartPath);
        _webViewManager.Navigate(StartPath);

        Task.Run(async () =>
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);
            BlazorWebViewInitialized?.Invoke(this, EventArgs.Empty);
        });
    }

    private static IFileProvider CreateFileProvider(string contentRootDir)
    {
        string contentRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        string bundleRootDir = Path.Combine(contentRoot, contentRootDir);
        var physicalProvider = new PhysicalFileProvider(bundleRootDir);
        var embeddedProvider = new BlazorEmbeddedFileProvider();
        return new CompositeFileProvider(physicalProvider, embeddedProvider);
    }

    protected override void InternalDispose()
    {
        throw new NotImplementedException();
    }

    // [LoggerMessage(EventId = 0, Level = LogLevel.Debug,
    //     Message =
    //         "Creating file provider at content root '{contentRootDir}', using host page relative path '{hostPageRelativePath}'.")]
    // partial void LogCreatingFileProvider(string contentRootDir, string hostPageRelativePath);

    // [LoggerMessage(EventId = 1, Level = LogLevel.Debug,
    //     Message =
    //         "Adding root component '{componentTypeName}' with selector '{componentSelector}'. Number of parameters: {parameterCount}")]
    // partial void LogAddingRootComponent(string componentTypeName, string componentSelector, int parameterCount);

    // [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Starting initial navigation to '{startPath}'.")]
    // partial void LogStartingInitialNavigation(string startPath);


}
