using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia;
using Xilium.CefGlue.Avalonia;
using Xilium.CefGlue.Common.Handlers;

namespace Blazor.Hybrid.Avalonia;

public partial class MainView : UserControl
{
    private AvaloniaCefBrowser browser;
    public MainView()
    {
        
        AvaloniaXamlLoader.Load(this);
        InitializeComponent();
        var browserWrapper = this.FindControl<Decorator>("browserWrapper");

        browser = new AvaloniaCefBrowser();
        browser.Address = "https://www.baidu.com";
        //browser.RegisterJavascriptObject(new BindingTestClass(), "boundBeforeLoadObject");
        //browser.LoadStart += OnBrowserLoadStart;
        //browser.TitleChanged += OnBrowserTitleChanged;
        //browser.LifeSpanHandler = new BrowserLifeSpanHandler();
        browserWrapper.Child = browser;
        
    }
}
