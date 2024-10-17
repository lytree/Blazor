using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using WebViewControl;

namespace Blazor.Hybrid.Avalonia.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        
        AvaloniaXamlLoader.Load(this);

        //DataContext = new MainWindowViewModel(this.FindControl<WebView>("webview"));
        InitializeComponent();
    }
}
