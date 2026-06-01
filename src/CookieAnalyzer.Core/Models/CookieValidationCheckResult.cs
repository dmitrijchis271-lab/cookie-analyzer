namespace CookieAnalyzer.Core.Models;

/// <summary>
/// Результат проверки валидности cookie на сайте
/// </summary>
public class CookieValidationCheckResult
{
    /// <summary>
    /// Домен/URL для проверки
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Cookie для проверки
    /// </summary>
    public Cookie Cookie { get; set; } = new();

    /// <summary>
    /// Сайт доступен
    /// </summary>
    public bool SiteAccessible { get; set; }

    /// <summary>
    /// Cookie валидна (работает на сайте)
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Статус проверки (например: 200 OK, 401 Unauthorized и т.д.)
    /// </summary>
    public string StatusCode { get; set; } = string.Empty;

    /// <summary>
    /// Сообщение об ошибке (если есть)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Время проверки
    /// </summary>
    public DateTime CheckTime { get; set; }

    /// <summary>
    /// Время отклика сайта (мс)
    /// </summary>
    public long ResponseTimeMs { get; set; }
}
