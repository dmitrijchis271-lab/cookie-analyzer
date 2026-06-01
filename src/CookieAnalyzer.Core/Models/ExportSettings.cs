namespace CookieAnalyzer.Core.Models;

/// <summary>
/// Settings for exporting analysis results
/// </summary>
public class ExportSettings
{
    /// <summary>
    /// Output directory path
    /// </summary>
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Export as CSV
    /// </summary>
    public bool ExportCsv { get; set; } = true;

    /// <summary>
    /// Export as JSON
    /// </summary>
    public bool ExportJson { get; set; } = true;

    /// <summary>
    /// Include only expired cookies
    /// </summary>
    public bool ExpiredOnly { get; set; }

    /// <summary>
    /// Include only valid cookies
    /// </summary>
    public bool ValidOnly { get; set; }

    /// <summary>
    /// Output filename without extension
    /// </summary>
    public string OutputFileName { get; set; } = "cookie_analysis_report";
}
