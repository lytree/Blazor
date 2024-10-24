using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia;
using Xilium.CefGlue.Avalonia;
using Xilium.CefGlue.Common.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Hybrid.Avalonia;

public partial class MainView : UserControl
{
    private BlazorWebView browser;



    private readonly ServiceCollection _serviceCollection = new();
    public MainView()
    {
        ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider();

        AvaloniaXamlLoader.Load(this);
        InitializeComponent();
        var browserWrapper = this.FindControl<Decorator>("browserWrapper");

        browser = new BlazorWebView(serviceProvider,false);
        browser.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(App) });
        browser.HostPage = "wwwroot/index.html";
        browser.ShowDeveloperTools();
        //browser.RegisterJavascriptObject(new BindingTestClass(), "boundBeforeLoadObject");
        //browser.RegisterJavascriptObject(new BindingTestClass(), "boundBeforeLoadObject");
        //browser.LoadStart += OnBrowserLoadStart;
        //browser.TitleChanged += OnBrowserTitleChanged;
        //browser.LifeSpanHandler = new BrowserLifeSpanHandler();
        browserWrapper.Child = browser;
        
    }
}
