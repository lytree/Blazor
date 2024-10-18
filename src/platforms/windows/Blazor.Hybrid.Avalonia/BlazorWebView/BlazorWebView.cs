using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blazor.Shared.Core;
using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Xilium.CefGlue;
using Xilium.CefGlue.Avalonia;

namespace Blazor.Hybrid.Avalonia;
internal sealed partial class BlazorWebView : AvaloniaCefBrowser
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


    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly CefSchemeHandlerFactory _appSchemeHandler;
    private readonly bool _enabledDeveloperTools;


    private BlazorWebViewManager? _webViewManager;
    
    internal BlazorWebView(IServiceProvider serviceProvider, bool enableDeveloperTools)
    {
        Guard.IsNotNull(serviceProvider);
        _serviceProvider = serviceProvider;

        _enabledDeveloperTools = enableDeveloperTools;


        RootComponents.CollectionChanged += RootComponentsOnCollectionChanged;

    }

    internal string? HostPage
    {
        get => Address;
        set
        {
            Address = value;
            StartWebViewCoreIfPossible();
        }
    }
    /// <summary>
    /// Gets or sets the path for initial navigation within the Blazor navigation context when the Blazor component is finished loading.
    /// </summary>
    internal string StartPath { get; set; } = "/";
    internal RootComponentsCollection RootComponents { get; } = new();


    /// <summary>
    /// Allows customizing how links are opened.
    /// By default, opens internal links in the webview and external links in an external app.
    /// </summary>
    internal event EventHandler<UrlLoadingEventArgs>? UrlLoading;

    internal event EventHandler? BlazorWebViewInitialized;
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
            });
        }
    }
    private void StartWebViewCoreIfPossible()
    {
        if (Address == null || _webViewManager != null)
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
            //await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);
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

}
