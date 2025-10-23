using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtk;
using WebKit;
using Settings = WebKit.Settings;
using Blazor.Hybrid.Linux.GTK.BlazorWebView;
using System.Runtime.Versioning;
using Blazor.Hybrid.Linux.Core;
using Blazor.Shared.Core;
namespace Blazor.Hybrid.Linux;


[SupportedOSPlatform("linux")]
internal class MainWindow
{
#if DEBUG
    private const bool EnableDeveloperTools = true;
#else
    private const bool EnableDeveloperTools = false;
#endif

    private readonly BlazorWebView _blazorGtkWebView;
    private readonly Window _window;

    internal MainWindow(IServiceProvider serviceProvider, Application application)
    {
        // serviceProvider.GetService<IMefProvider>()!.SatisfyImports(this);
        // Guard.IsNotNull(_themeListener);
        // Guard.IsNotNull(_titleBarInfoProvider);
        // _titleBarInfoProvider.PropertyChanged += TitleBarInfoProvider_PropertyChanged;

        _blazorGtkWebView = new BlazorWebView(serviceProvider, EnableDeveloperTools);
        // _blazorGtkWebView.BlazorWebViewInitialized += OnBlazorWebViewInitialized;

        // Create and open main window.
        _window = ApplicationWindow.New(application);
        // _window.Title = _titleBarInfoProvider.TitleWithToolName ?? string.Empty;
        _window.SetDefaultSize(1280, 800);
        _window.SetChild(_blazorGtkWebView.View);

        var windowService = (WindowService)serviceProvider.GetService<IWindowService>()!;
        // ((ThemeListener)_themeListener).SetMainWindow(_window, windowService);
        windowService.SetMainWindow(_window);
        // ((FileStorage)_fileStorage).MainWindow = _window;
        // ((FontProvider)_fontProvider).MainWindow = _window;

        // Navigate to our Blazor webpage.
        _blazorGtkWebView.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(App) });
        _blazorGtkWebView.HostPage = "wwwroot/index.html";

        _window.Show();
    }

    // private void OnBlazorWebViewInitialized(object? sender, EventArgs args)
    // {
    //     InitializeLowPriorityServices();
    // }

    // private void TitleBarInfoProvider_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    // {
    //     if (e.PropertyName == nameof(TitleBarInfoProvider.TitleWithToolName))
    //     {
    //         _window.Title = _titleBarInfoProvider.TitleWithToolName ?? string.Empty;
    //     }
    // }

    // private void InitializeLowPriorityServices()
    // {
    //     // Treat command line arguments.
    //     _commandLineLauncherService.HandleCommandLineArguments();
    // }
}
