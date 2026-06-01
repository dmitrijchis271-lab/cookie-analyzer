namespace CookieAnalyzer.UI.ViewModels;

using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using CookieAnalyzer.Core.Models;
using CookieAnalyzer.Core.Services;
using Serilog;

public class MainWindowViewModel : ViewModelBase
{
    private readonly DirectoryScannerService _scannerService;
    private readonly ExportService _exportService;
    private readonly CookieValidationService _validationService;
    private readonly ValidationResultStorageService _storageService;
    private readonly ILogger _logger;

    private string _selectedDirectory = string.Empty;
    private ObservableCollection<CookieAnalysisResult> _analysisResults = new();
    private ObservableCollection<Cookie> _displayedCookies = new();
    private ObservableCollection<CookieValidationCheckResult> _validationResults = new();
    private bool _isScanning;
    private bool _isValidating;
    private string _statusMessage = "Готово";
    private int _progress;
    private string _progressText = "";
    private bool _showExpiredOnly;
    private bool _showSecureOnly;
    private bool _enableValidation = true;
    private string _validOutputDirectory = string.Empty;

    public MainWindowViewModel(DirectoryScannerService scannerService, 
        ExportService exportService,
        CookieValidationService validationService,
        ValidationResultStorageService storageService)
    {
        _scannerService = scannerService ?? throw new ArgumentNullException(nameof(scannerService));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _logger = Log.ForContext<MainWindowViewModel>();

        BrowseCommand = ReactiveCommand.CreateFromTask(BrowseDirectory);
        ScanCommand = ReactiveCommand.CreateFromTask(
            ScanDirectory,
            this.WhenAnyValue(
                x => x.SelectedDirectory,
                x => x.IsScanning,
                x => x.IsValidating,
                (dir, scanning, validating) => !string.IsNullOrWhiteSpace(dir) && !scanning && !validating));
        ValidateCommand = ReactiveCommand.CreateFromTask(
            ValidateCookies,
            this.WhenAnyValue(
                x => x.AnalysisResults.Count,
                x => x.IsValidating,
                (count, validating) => count > 0 && !validating));
        ExportCommand = ReactiveCommand.CreateFromTask(
            Export,
            this.WhenAnyValue(x => x.AnalysisResults.Count, count => count > 0));
        RefreshCommand = ReactiveCommand.Create(RefreshDisplay);
        BrowseValidOutputCommand = ReactiveCommand.CreateFromTask(BrowseValidOutputDirectory);
    }

    public ReactiveCommand<Unit, Unit> BrowseCommand { get; }
    public ReactiveCommand<Unit, Unit> ScanCommand { get; }
    public ReactiveCommand<Unit, Unit> ValidateCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> BrowseValidOutputCommand { get; }

    public string SelectedDirectory
    {
        get => _selectedDirectory;
        set => this.RaiseAndSetIfChanged(ref _selectedDirectory, value);
    }

    public ObservableCollection<CookieAnalysisResult> AnalysisResults
    {
        get => _analysisResults;
        set => this.RaiseAndSetIfChanged(ref _analysisResults, value);
    }

    public ObservableCollection<Cookie> DisplayedCookies
    {
        get => _displayedCookies;
        set => this.RaiseAndSetIfChanged(ref _displayedCookies, value);
    }

    public ObservableCollection<CookieValidationCheckResult> ValidationResults
    {
        get => _validationResults;
        set => this.RaiseAndSetIfChanged(ref _validationResults, value);
    }

    public bool IsScanning
    {
        get => _isScanning;
        set => this.RaiseAndSetIfChanged(ref _isScanning, value);
    }

    public bool IsValidating
    {
        get => _isValidating;
        set => this.RaiseAndSetIfChanged(ref _isValidating, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public int Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    public string ProgressText
    {
        get => _progressText;
        set => this.RaiseAndSetIfChanged(ref _progressText, value);
    }

    public bool ShowExpiredOnly
    {
        get => _showExpiredOnly;
        set
        {
            this.RaiseAndSetIfChanged(ref _showExpiredOnly, value);
            RefreshDisplay();
        }
    }

    public bool ShowSecureOnly
    {
        get => _showSecureOnly;
        set
        {
            this.RaiseAndSetIfChanged(ref _showSecureOnly, value);
            RefreshDisplay();
        }
    }

    public bool EnableValidation
    {
        get => _enableValidation;
        set => this.RaiseAndSetIfChanged(ref _enableValidation, value);
    }

    public string ValidOutputDirectory
    {
        get => _validOutputDirectory;
        set => this.RaiseAndSetIfChanged(ref _validOutputDirectory, value);
    }

    public int TotalCookies => AnalysisResults.Sum(r => r.Cookies.Count);
    public int ExpiredCookies => AnalysisResults.Sum(r => r.GetExpiredCookies().Count);
    public int SecureCookies => AnalysisResults.Sum(r => r.GetSecureCookies().Count);
    public int HttpOnlyCookies => AnalysisResults.Sum(r => r.GetHttpOnlyCookies().Count);
    public int ValidCookies => ValidationResults.Count(r => r.IsValid);
    public int InvalidCookies => ValidationResults.Count(r => !r.IsValid);

    private async Task BrowseDirectory()
    {
        var dialog = new Avalonia.Controls.OpenFolderDialog
        {
            Title = "Выбрать папку с файлами cookie"
        };

        var window = (Avalonia.Controls.Window?)Avalonia.Application.Current?.ApplicationLifetime?
            .GetType().GetProperty("MainWindow")?.GetValue(Avalonia.Application.Current?.ApplicationLifetime);

        if (window != null)
        {
            var result = await dialog.ShowAsync(window);
            if (!string.IsNullOrWhiteSpace(result))
            {
                SelectedDirectory = result;
                StatusMessage = $"Выбрана папка: {result}";
            }
        }
    }

    private async Task BrowseValidOutputDirectory()
    {
        var dialog = new Avalonia.Controls.OpenFolderDialog
        {
            Title = "Выбрать папку для сохранения валидных cookie"
        };

        var window = (Avalonia.Controls.Window?)Avalonia.Application.Current?.ApplicationLifetime?
            .GetType().GetProperty("MainWindow")?.GetValue(Avalonia.Application.Current?.ApplicationLifetime);

        if (window != null)
        {
            var result = await dialog.ShowAsync(window);
            if (!string.IsNullOrWhiteSpace(result))
            {
                ValidOutputDirectory = result;
                StatusMessage = $"Папка сохранения выбрана: {result}";
            }
        }
    }

    private async Task ScanDirectory()
    {
        if (string.IsNullOrWhiteSpace(SelectedDirectory))
            return;

        IsScanning = true;
        StatusMessage = "Сканирование папки...";
        AnalysisResults.Clear();
        DisplayedCookies.Clear();
        ValidationResults.Clear();

        try
        {
            var progress = new Progress<string>(message =>
            {
                ProgressText = message;
                _logger.Information(message);
            });

            var results = await _scannerService.ScanDirectoryAsync(SelectedDirectory, progress);
            AnalysisResults = new ObservableCollection<CookieAnalysisResult>(results);

            RefreshDisplay();

            var totalCookies = TotalCookies;
            var expiredCookies = ExpiredCookies;
            StatusMessage = $"✓ Сканирование завершено. Найдено {totalCookies} cookie, {expiredCookies} истекших";

            _logger.Information("Scan completed. Total: {Total}, Expired: {Expired}", totalCookies, expiredCookies);
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Ошибка: {ex.Message}";
            _logger.Error(ex, "Scan failed");
        }
        finally
        {
            IsScanning = false;
        }
    }

    private async Task ValidateCookies()
    {
        if (AnalysisResults.Count == 0)
        {
            StatusMessage = "✗ Нет cookie для проверки. Сначала выполните сканирование.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ValidOutputDirectory))
        {
            StatusMessage = "✗ Выберите папку для сохранения результатов валидации.";
            return;
        }

        IsValidating = true;
        StatusMessage = "Проверка валидности cookie на сайтах...";
        ValidationResults.Clear();
        Progress = 0;

        try
        {
            var settings = new CookieValidationSettings
            {
                EnableValidation = EnableValidation,
                RequestTimeoutSeconds = 10,
                ValidCookiesOutputDirectory = ValidOutputDirectory,
                CreateValidationReport = true,
                MaxConcurrentRequests = 3,
                CheckSiteAvailabilityFirst = true
            };

            var cookiesToValidate = new List<(Cookie cookie, string domain)>();
            foreach (var result in AnalysisResults)
            {
                foreach (var cookie in result.Cookies)
                {
                    cookiesToValidate.Add((cookie, cookie.Domain));
                }
            }

            var progress = new Progress<string>(message =>
            {
                ProgressText = message;
                Progress = (ValidationResults.Count * 100) / cookiesToValidate.Count;
            });

            var validationResults = await _validationService.ValidateCookiesBatchAsync(
                cookiesToValidate,
                settings,
                progress);

            ValidationResults = new ObservableCollection<CookieValidationCheckResult>(validationResults);

            // Сохраняем валидные cookie
            await _storageService.SaveValidCookiesAsync(validationResults, ValidOutputDirectory);

            // Создаём отчёты
            await _storageService.CreateValidationReportAsync(validationResults, ValidOutputDirectory);
            await _storageService.CreateSiteValidationReportsAsync(validationResults, ValidOutputDirectory);

            var validCount = validationResults.Count(r => r.IsValid);
            var invalidCount = validationResults.Count(r => !r.IsValid);
            var accessibleCount = validationResults.Count(r => r.SiteAccessible);

            StatusMessage = $"✓ Проверка завершена. Валидных: {validCount}, Невалидных: {invalidCount}, Доступных сайтов: {accessibleCount}";
            
            _logger.Information(
                "Validation completed. Valid: {Valid}, Invalid: {Invalid}, Accessible sites: {Accessible}",
                validCount, invalidCount, accessibleCount);
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Ошибка при проверке: {ex.Message}";
            _logger.Error(ex, "Validation failed");
        }
        finally
        {
            IsValidating = false;
            Progress = 0;
        }
    }

    private async Task Export()
    {
        try
        {
            var settings = new ExportSettings
            {
                OutputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CookieAnalyzerReports"),
                ExportCsv = true,
                ExportJson = true,
                OutputFileName = $"cookie_report_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            await _exportService.ExportResultsAsync(AnalysisResults.ToList(), settings);
            StatusMessage = $"✓ Отчет успешно экспортирован в {settings.OutputDirectory}";
            _logger.Information("Export completed to {OutputDirectory}", settings.OutputDirectory);
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Ошибка экспорта: {ex.Message}";
            _logger.Error(ex, "Export failed");
        }
    }

    private void RefreshDisplay()
    {
        var cookies = new List<Cookie>();

        foreach (var result in AnalysisResults)
        {
            cookies.AddRange(result.Cookies);
        }

        if (ShowExpiredOnly)
            cookies = cookies.Where(c => c.IsExpired).ToList();

        if (ShowSecureOnly)
            cookies = cookies.Where(c => c.Secure).ToList();

        DisplayedCookies = new ObservableCollection<Cookie>(cookies);
        this.RaisePropertyChanged(nameof(TotalCookies));
        this.RaisePropertyChanged(nameof(ExpiredCookies));
        this.RaisePropertyChanged(nameof(SecureCookies));
        this.RaisePropertyChanged(nameof(HttpOnlyCookies));
    }
}
