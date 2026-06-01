namespace CookieAnalyzer.Core.Services;

using System.Collections.Concurrent;
using CookieAnalyzer.Core.Models;
using Serilog;

/// <summary>
/// Service for scanning directories and finding cookie files
/// </summary>
public class DirectoryScannerService
{
    private readonly ILogger _logger;
    private readonly CookieReaderService _cookieReaderService;

    public DirectoryScannerService(CookieReaderService cookieReaderService)
    {
        _cookieReaderService = cookieReaderService ?? throw new ArgumentNullException(nameof(cookieReaderService));
        _logger = Log.ForContext<DirectoryScannerService>();
    }

    /// <summary>
    /// Recursively scans directory for JSON files and reads cookies
    /// </summary>
    public async Task<List<CookieAnalysisResult>> ScanDirectoryAsync(string directoryPath, 
        IProgress<string>? progress = null, 
        CancellationToken cancellationToken = default)
    {
        var results = new ConcurrentBag<CookieAnalysisResult>();

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                _logger.Error("Directory not found: {DirectoryPath}", directoryPath);
                return new List<CookieAnalysisResult>();
            }

            var jsonFiles = GetJsonFilesRecursive(directoryPath);
            _logger.Information("Found {FileCount} JSON files in {DirectoryPath}", jsonFiles.Count, directoryPath);

            // Process files in parallel
            var tasks = jsonFiles.Select(async filePath =>
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                try
                {
                    progress?.Report($"Processing: {Path.GetFileName(filePath)}");
                    var result = await _cookieReaderService.ReadCookiesFromFileAsync(filePath);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error processing file {FilePath}", filePath);
                }
            });

            await Task.WhenAll(tasks);

            _logger.Information("Scan completed. Processed {ResultCount} files", results.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error scanning directory {DirectoryPath}", directoryPath);
        }

        return results.ToList();
    }

    /// <summary>
    /// Recursively gets all JSON files in directory
    /// </summary>
    private List<string> GetJsonFilesRecursive(string directoryPath)
    {
        var jsonFiles = new List<string>();

        try
        {
            var files = Directory.GetFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly);
            jsonFiles.AddRange(files);

            var subdirectories = Directory.GetDirectories(directoryPath);
            foreach (var subdirectory in subdirectories)
            {
                jsonFiles.AddRange(GetJsonFilesRecursive(subdirectory));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Warning(ex, "Access denied to directory {DirectoryPath}", directoryPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error accessing directory {DirectoryPath}", directoryPath);
        }

        return jsonFiles;
    }
}
