using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CookieAnalyzer.UI.ViewModels;

namespace CookieAnalyzer.UI;

public partial class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
