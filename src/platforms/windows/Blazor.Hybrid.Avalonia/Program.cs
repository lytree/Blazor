using System;
using System.Linq;
using System.Threading;

using Avalonia;
using Xilium.CefGlue;
using Xilium.CefGlue.Common;
using Xilium.CefGlue.Common.Shared;

namespace Blazor.Hybrid.Avalonia;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        var builder = BuildAvaloniaApp()
        //if(args.Contains("--drm"))
        //{
        //    SilenceConsole();

        //    // If Card0, Card1 and Card2 all don't work. You can also try:                 
        //    // return builder.StartLinuxFbDev(args);
        //    // return builder.StartLinuxDrm(args, "/dev/dri/card1");
        //    return builder.StartLinuxDrm(args, "/dev/dri/card1", 1D);
        //}
        .AfterSetup(_ => CefRuntimeLoader.Initialize(new CefSettings()
        {
#if WINDOWLESS
                          WindowlessRenderingEnabled = true
#else
            WindowlessRenderingEnabled = false
#endif
        }, customSchemes: new[] {
                        new CustomScheme()
                        {
                            SchemeName = "app",
                            SchemeHandlerFactory = new BlazorSchemeHandler()
                        }
                      }));

        return builder.StartWithClassicDesktopLifetime(args);
    }


    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
        .With(new Win32PlatformOptions()
        {
            // CompositionMode = new [] { Win32CompositionMode.WinUIComposition }
        })
                      .AfterSetup(_ => CefRuntimeLoader.Initialize(new CefSettings()
                      {
#if WINDOWLESS
                          WindowlessRenderingEnabled = true
#else
                          WindowlessRenderingEnabled = false
#endif
                      },
                      customSchemes: new[] {
                        new CustomScheme()
                        {
                            SchemeName = "app",
                            SchemeHandlerFactory = new AppSchemeHandler()
                        }
                        }))
            .WithInterFont()
            .LogToTrace();


    private static void SilenceConsole()
    {
        new Thread(() =>
            {
                Console.CursorVisible = false;
                while (true)
                    Console.ReadKey(true);
            })
        { IsBackground = true }.Start();
    }
}
