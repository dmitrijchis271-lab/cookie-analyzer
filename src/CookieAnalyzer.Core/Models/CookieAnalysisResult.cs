namespace CookieAnalyzer.Core.Models;

/// <summary>
/// Result of analyzing cookies from a file
/// </summary>
public class CookieAnalysisResult
{
    /// <summary>
    /// Path to the source file
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// List of cookies found
    /// </summary>
    public List<Cookie> Cookies { get; set; } = new();

    /// <summary>
    /// List of validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Indicates if the file was successfully parsed
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Timestamp when the analysis was performed
    /// </summary>
    public DateTime AnalysisTime { get; set; }

    /// <summary>
    /// Gets expired cookies
    /// </summary>
    public List<Cookie> GetExpiredCookies() => Cookies.Where(c => c.IsExpired).ToList();

    /// <summary>
    /// Gets valid cookies
    /// </summary>
    public List<Cookie> GetValidCookies() => Cookies.Where(c => !c.IsExpired).ToList();

    /// <summary>
    /// Gets secure cookies
    /// </summary>
    public List<Cookie> GetSecureCookies() => Cookies.Where(c => c.Secure).ToList();

    /// <summary>
    /// Gets HTTP-only cookies
    /// </summary>
    public List<Cookie> GetHttpOnlyCookies() => Cookies.Where(c => c.HttpOnly).ToList();
}
