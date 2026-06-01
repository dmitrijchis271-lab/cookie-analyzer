namespace CookieAnalyzer.Core.Models;

/// <summary>
/// Represents a single cookie with all its properties
/// </summary>
public class Cookie
{
    /// <summary>
    /// Domain associated with the cookie
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Name of the cookie
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Value of the cookie
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Expiration date of the cookie (Unix timestamp)
    /// </summary>
    public long ExpirationDate { get; set; }

    /// <summary>
    /// Indicates if the cookie is marked as Secure
    /// </summary>
    public bool Secure { get; set; }

    /// <summary>
    /// Indicates if the cookie is marked as HttpOnly
    /// </summary>
    public bool HttpOnly { get; set; }

    /// <summary>
    /// Path for which the cookie is valid
    /// </summary>
    public string Path { get; set; } = "/";

    /// <summary>
    /// Session cookie flag
    /// </summary>
    public bool Session { get; set; }

    /// <summary>
    /// SameSite attribute
    /// </summary>
    public string? SameSite { get; set; }

    /// <summary>
    /// Gets the expiration date as DateTime
    /// </summary>
    public DateTime ExpirationDateTime => UnixTimeStampToDateTime(ExpirationDate);

    /// <summary>
    /// Checks if the cookie has expired
    /// </summary>
    public bool IsExpired => ExpirationDateTime < DateTime.UtcNow;

    /// <summary>
    /// Gets the days until expiration (negative if expired)
    /// </summary>
    public int DaysUntilExpiration
    {
        get
        {
            var daysRemaining = (ExpirationDateTime - DateTime.UtcNow).Days;
            return daysRemaining;
        }
    }

    /// <summary>
    /// Converts Unix timestamp to DateTime
    /// </summary>
    private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        if (unixTimeStamp == 0)
            return DateTime.MaxValue;

        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
        return dateTime;
    }

    /// <summary>
    /// Validates the cookie data structure
    /// </summary>
    public ValidationResult Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Domain))
            errors.Add("Domain is required");

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Name is required");

        if (ExpirationDate < 0)
            errors.Add("ExpirationDate cannot be negative");

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}

/// <summary>
/// Result of cookie validation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
