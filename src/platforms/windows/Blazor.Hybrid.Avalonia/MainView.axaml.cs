using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Xilium.CefGlue.Avalonia;
using Xilium.CefGlue.Common.Handlers;
using Xilium.CefGlue;
using Dispatcher = Avalonia.Threading.Dispatcher;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Hybrid.Avalonia;

public partial class MainView : UserControl
{

    private BlazorWebView browser;
    public MainView()
    {

        //DataContext = new MainWindowViewModel(this.FindControl<WebView>("webview"));
        InitializeComponent();
        var browserWrapper = this.FindControl<Decorator>("browserWrapper");
        var serviceCollection = new ServiceCollection();
        browser = new BlazorWebView(serviceCollection.BuildServiceProvider(),true);
        browser.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(App) });
        browser.HostPage = "wwwroot/index.html";
        OpenDevTools();
        browserWrapper.Child = browser;
    }

    static Task<object> AsyncCallNativeMethod(Func<object> nativeMethod)
    {
        return Task.Run(() =>
        {
            var result = nativeMethod.Invoke();
            if (result is Task task)
            {
                if (task.GetType().IsGenericType)
                {
                    return ((dynamic)task).Result;
                }

                return task;
            }

            return result;
        });
    }

    public event Action<string> TitleChanged;

    private void OnBrowserTitleChanged(object sender, string title)
    {
        TitleChanged?.Invoke(title);
    }

    private void OnBrowserLoadStart(object sender, Xilium.CefGlue.Common.Events.LoadStartEventArgs e)
    {
        if (e.Frame.Browser.IsPopup || !e.Frame.IsMain)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            var addressTextBox = this.FindControl<TextBox>("addressTextBox");

            addressTextBox.Text = e.Frame.Url;
        });
    }

    private void OnAddressTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            browser.Address = ((TextBox)sender).Text;
        }
    }


    public void BindJavascriptObject()
    {
        //const string TestObject = "dotNetObject";

        //var obj = new BindingTestClass();
        //browser.RegisterJavascriptObject(obj, TestObject, AsyncCallNativeMethod);

        //var methods = obj.GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
        //                           .Where(m => m.GetParameters().Length == 0)
        //                           .Select(m => m.Name.Substring(0, 1).ToLowerInvariant() + m.Name.Substring(1));

        //var script = "(function () {" +
        //    "let calls = [];" +
        //    string.Join("", methods.Select(m => $"calls.push({{ name: '{m}', promise: {TestObject}.{m}() }});")) +
        //    $"calls.push({{ name: 'asyncGetObjectWithParams', promise: {TestObject}.asyncGetObjectWithParams('a string') }});" +
        //    $"calls.push({{ name: 'getObjectWithParams', promise: {TestObject}.getObjectWithParams(5, 'a string', {{ Name: 'obj name', Value: 10 }}, [ 1, 2 ]) }});" +
        //    "calls.forEach(c => c.promise.then(r => console.log(c.name + ': ' + JSON.stringify(r))).catch(e => console.log(e)));" +
        //    "})()";

        //browser.ExecuteJavaScript(script);
    }

    public void OpenDevTools()
    {
        browser.ShowDeveloperTools();
    }

    public void Dispose()
    {
        browser.Dispose();
    }

    private class BrowserLifeSpanHandler : LifeSpanHandler
    {
        protected override bool OnBeforePopup(
            CefBrowser browser,
            CefFrame frame,
            string targetUrl,
            string targetFrameName,
            CefWindowOpenDisposition targetDisposition,
            bool userGesture,
            CefPopupFeatures popupFeatures,
            CefWindowInfo windowInfo,
            ref CefClient client,
            CefBrowserSettings settings,
            ref CefDictionaryValue extraInfo,
            ref bool noJavascriptAccess)
        {
            var bounds = windowInfo.Bounds;
            Dispatcher.UIThread.Post(() =>
            {
                var window = new Window();
                var popupBrowser = new AvaloniaCefBrowser();
                popupBrowser.Address = targetUrl;
                window.Content = popupBrowser;
                window.Position = new PixelPoint(bounds.X, bounds.Y);
                window.Height = bounds.Height;
                window.Width = bounds.Width;
                window.Title = targetUrl;
                window.Show();
            });
            return true;
        }
    }
}
