using CookieAnalyzer.Core.Services;
using CookieAnalyzer.UI.ViewModels;
using Serilog;

namespace CookieAnalyzer.UI;

public class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ConfigureLogging();
    }

    private void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/cookie-analyzer-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopApplicationLifetime desktop)
        {
            var cookieReaderService = new CookieReaderService();
            var directoryScannerService = new DirectoryScannerService(cookieReaderService);
            var exportService = new ExportService();
            var validationService = new CookieValidationService();
            var storageService = new ValidationResultStorageService();
            
            var mainViewModel = new MainWindowViewModel(
                directoryScannerService, 
                exportService,
                validationService,
                storageService);
            desktop.MainWindow = new MainWindow { DataContext = mainViewModel };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
