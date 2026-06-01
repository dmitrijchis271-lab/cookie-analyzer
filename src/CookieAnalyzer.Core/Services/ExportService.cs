namespace CookieAnalyzer.Core.Services;

using System.Globalization;
using System.Text.Json;
using CookieAnalyzer.Core.Models;
using CsvHelper;
using Serilog;

/// <summary>
/// Service for exporting analysis results to CSV and JSON formats
/// </summary>
public class ExportService
{
    private readonly ILogger _logger;

    public ExportService()
    {
        _logger = Log.ForContext<ExportService>();
    }

    /// <summary>
    /// Exports analysis results to CSV and/or JSON
    /// </summary>
    public async Task ExportResultsAsync(List<CookieAnalysisResult> analysisResults, ExportSettings settings)
    {
        try
        {
            if (!Directory.Exists(settings.OutputDirectory))
            {
                Directory.CreateDirectory(settings.OutputDirectory);
                _logger.Information("Created output directory: {OutputDirectory}", settings.OutputDirectory);
            }

            var allCookies = CollectCookies(analysisResults, settings);

            if (settings.ExportCsv)
            {
                await ExportToCsvAsync(allCookies, settings);
            }

            if (settings.ExportJson)
            {
                await ExportToJsonAsync(allCookies, settings);
            }

            _logger.Information("Export completed successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error exporting results");
            throw;
        }
    }

    /// <summary>
    /// Exports cookies to CSV file
    /// </summary>
    private async Task ExportToCsvAsync(List<Cookie> cookies, ExportSettings settings)
    {
        var csvPath = Path.Combine(settings.OutputDirectory, $"{settings.OutputFileName}.csv");

        try
        {
            await using var writer = new StreamWriter(csvPath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteHeader<CookieCsvRecord>();
            await csv.NextRecordAsync();

            foreach (var cookie in cookies)
            {
                var record = new CookieCsvRecord
                {
                    Domain = cookie.Domain,
                    Name = cookie.Name,
                    Value = cookie.Value,
                    ExpirationDate = cookie.ExpirationDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Secure = cookie.Secure ? "Yes" : "No",
                    HttpOnly = cookie.HttpOnly ? "Yes" : "No",
                    Path = cookie.Path,
                    Session = cookie.Session ? "Yes" : "No",
                    SameSite = cookie.SameSite ?? "Not Set",
                    IsExpired = cookie.IsExpired ? "Expired" : "Valid",
                    DaysUntilExpiration = cookie.DaysUntilExpiration
                };

                csv.WriteRecord(record);
                await csv.NextRecordAsync();
            }

            _logger.Information("CSV export completed: {CsvPath}", csvPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error exporting to CSV: {CsvPath}", csvPath);
            throw;
        }
    }

    /// <summary>
    /// Exports cookies to JSON file
    /// </summary>
    private async Task ExportToJsonAsync(List<Cookie> cookies, ExportSettings settings)
    {
        var jsonPath = Path.Combine(settings.OutputDirectory, $"{settings.OutputFileName}.json");

        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(cookies, options);
            await File.WriteAllTextAsync(jsonPath, json);

            _logger.Information("JSON export completed: {JsonPath}", jsonPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error exporting to JSON: {JsonPath}", jsonPath);
            throw;
        }
    }

    /// <summary>
    /// Collects cookies from analysis results based on filter settings
    /// </summary>
    private List<Cookie> CollectCookies(List<CookieAnalysisResult> analysisResults, ExportSettings settings)
    {
        var cookies = new List<Cookie>();

        foreach (var result in analysisResults)
        {
            var resultCookies = result.Cookies;

            if (settings.ExpiredOnly)
                resultCookies = result.GetExpiredCookies();
            else if (settings.ValidOnly)
                resultCookies = result.GetValidCookies();

            cookies.AddRange(resultCookies);
        }

        return cookies.DistinctBy(c => $"{c.Domain}_{c.Name}").ToList();
    }
}

/// <summary>
/// CSV record for cookie export
/// </summary>
public class CookieCsvRecord
{
    public string Domain { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ExpirationDate { get; set; } = string.Empty;
    public string Secure { get; set; } = string.Empty;
    public string HttpOnly { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Session { get; set; } = string.Empty;
    public string SameSite { get; set; } = string.Empty;
    public string IsExpired { get; set; } = string.Empty;
    public int DaysUntilExpiration { get; set; }
}
