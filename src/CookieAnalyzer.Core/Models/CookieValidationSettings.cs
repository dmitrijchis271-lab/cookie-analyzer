namespace CookieAnalyzer.Core.Models;

/// <summary>
/// Настройки для проверки валидности cookie
/// </summary>
public class CookieValidationSettings
{
    /// <summary>
    /// Включить проверку валидности cookie на сайтах
    /// </summary>
    public bool EnableValidation { get; set; } = true;

    /// <summary>
    /// Timeout для HTTP запросов (секунды)
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Папка для сохранения валидных cookie
    /// </summary>
    public string ValidCookiesOutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Создавать отчет о результатах проверки
    /// </summary>
    public bool CreateValidationReport { get; set; } = true;

    /// <summary>
    /// Максимальное количество одновременных запросов
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 3;

    /// <summary>
    /// Проверять доступность сайта перед проверкой cookie
    /// </summary>
    public bool CheckSiteAvailabilityFirst { get; set; } = true;
}
