namespace CookieAnalyzer.Core.Services;

using System.Text.Json;
using System.Text.Json.Serialization;
using CookieAnalyzer.Core.Models;
using Serilog;

/// <summary>
/// Service for reading and parsing cookie JSON files
/// </summary>
public class CookieReaderService
{
    private readonly ILogger _logger;

    public CookieReaderService()
    {
        _logger = Log.ForContext<CookieReaderService>();
    }

    /// <summary>
    /// Reads cookies from a JSON file
    /// </summary>
    public async Task<CookieAnalysisResult> ReadCookiesFromFileAsync(string filePath)
    {
        var result = new CookieAnalysisResult
        {
            FilePath = filePath,
            AnalysisTime = DateTime.UtcNow
        };

        try
        {
            if (!File.Exists(filePath))
            {
                result.Errors.Add($"File not found: {filePath}");
                result.IsSuccessful = false;
                _logger.Warning("File not found: {FilePath}", filePath);
                return result;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var cookies = ParseCookiesFromJson(json);

            // Validate each cookie
            foreach (var cookie in cookies)
            {
                var validation = cookie.Validate();
                if (!validation.IsValid)
                {
                    result.Errors.AddRange(validation.Errors.Select(e => $"{cookie.Name}: {e}"));
                }
                else
                {
                    result.Cookies.Add(cookie);
                }
            }

            result.IsSuccessful = true;
            _logger.Information("Successfully read {CookieCount} cookies from {FilePath}", 
                result.Cookies.Count, filePath);
        }
        catch (JsonException ex)
        {
            result.Errors.Add($"JSON parsing error: {ex.Message}");
            result.IsSuccessful = false;
            _logger.Error(ex, "JSON parsing error in file {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error reading file: {ex.Message}");
            result.IsSuccessful = false;
            _logger.Error(ex, "Error reading file {FilePath}", filePath);
        }

        return result;
    }

    /// <summary>
    /// Parses cookies from JSON string
    /// </summary>
    private List<Cookie> ParseCookiesFromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        // Try to parse as array first
        try
        {
            var cookies = JsonSerializer.Deserialize<List<Cookie>>(json, options);
            return cookies ?? new List<Cookie>();
        }
        catch
        {
            // If array fails, try to parse as object with "cookies" property
            try
            {
                var root = JsonSerializer.Deserialize<JsonElement>(json);
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("cookies", out var cookiesElement))
                {
                    var cookies = JsonSerializer.Deserialize<List<Cookie>>(cookiesElement.GetRawText(), options);
                    return cookies ?? new List<Cookie>();
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    var cookies = JsonSerializer.Deserialize<List<Cookie>>(json, options);
                    return cookies ?? new List<Cookie>();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error parsing JSON structure");
            }
        }

        return new List<Cookie>();
    }
}
