using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using WebViewControl;

namespace Blazor.Hybrid.Avalonia;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        WebView.Settings.OsrEnabled = false;
        WebView.Settings.LogFile = "ceflog.txt";
        
        AvaloniaXamlLoader.Load(this);
        Console.WriteLine("Starting...");
        InitializeComponent();
    }
}