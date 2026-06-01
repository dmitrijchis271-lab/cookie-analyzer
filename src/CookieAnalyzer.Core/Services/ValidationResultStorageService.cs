namespace CookieAnalyzer.Core.Services;

using CookieAnalyzer.Core.Models;
using System.Text;
using Serilog;

/// <summary>
/// Сервис для сохранения результатов проверки валидности cookie
/// </summary>
public class ValidationResultStorageService
{
    private readonly ILogger _logger;

    public ValidationResultStorageService()
    {
        _logger = Log.ForContext<ValidationResultStorageService>();
    }

    /// <summary>
    /// Сохраняет валидные cookie в отдельную папку
    /// </summary>
    public async Task SaveValidCookiesAsync(
        List<CookieValidationCheckResult> validationResults,
        string outputDirectory)
    {
        try
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var validCookies = validationResults.Where(r => r.IsValid).ToList();

            if (!validCookies.Any())
            {
                _logger.Information("No valid cookies to save");
                return;
            }

            // Группируем по доменам
            var cookiesByDomain = validCookies.GroupBy(r => r.Domain);

            foreach (var domainGroup in cookiesByDomain)
            {
                var domain = domainGroup.Key;
                var domainCookies = domainGroup.Select(r => r.Cookie).ToList();

                // Сохраняем как JSON
                var jsonFileName = SanitizeFileName($"{domain}_valid_cookies.json");
                var jsonPath = Path.Combine(outputDirectory, jsonFileName);

                var options = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = System.Text.Json.JsonSerializer.Serialize(domainCookies, options);
                await File.WriteAllTextAsync(jsonPath, json);

                _logger.Information("Saved {Count} valid cookies for {Domain} to {Path}",
                    domainCookies.Count, domain, jsonPath);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving valid cookies");
            throw;
        }
    }

    /// <summary>
    /// Создаёт подробный TXT отчёт о проверке cookie
    /// </summary>
    public async Task CreateValidationReportAsync(
        List<CookieValidationCheckResult> validationResults,
        string outputDirectory,
        string reportFileName = "validation_report")
    {
        try
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var reportPath = Path.Combine(outputDirectory, $"{reportFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            sb.AppendLine("                    ОТЧЁТ ПО ПРОВЕРКЕ ВАЛИДНОСТИ COOKIE");
            sb.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            sb.AppendLine($"Дата отчёта: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Всего проверено: {validationResults.Count}");
            sb.AppendLine($"Валидные: {validationResults.Count(r => r.IsValid)}");
            sb.AppendLine($"Невалидные: {validationResults.Count(r => !r.IsValid)}");
            sb.AppendLine($"Доступные сайты: {validationResults.Count(r => r.SiteAccessible)}");
            sb.AppendLine("═══════════════════════════════════════════════════════════════════════════════\n");

            // Группируем по доменам
            var groupedResults = validationResults.GroupBy(r => r.Domain);

            foreach (var domainGroup in groupedResults)
            {
                var domain = domainGroup.Key;
                var domainResults = domainGroup.ToList();
                var validCount = domainResults.Count(r => r.IsValid);

                sb.AppendLine($"\n█████ ДОМЕН: {domain} █████");
                sb.AppendLine($"├─ Доступность: {(domainResults.First().SiteAccessible ? "✓ Доступен" : "✗ Недоступен")}");
                sb.AppendLine($"├─ Валидных cookie: {validCount}/{domainResults.Count}");
                sb.AppendLine("├─────────────────────────────────────────────────────────────────");

                foreach (var result in domainResults)
                {
                    sb.AppendLine($"\n  📌 Cookie: {result.Cookie.Name}");
                    sb.AppendLine($"  ├─ Значение: {(result.Cookie.Value.Length > 50 ? result.Cookie.Value.Substring(0, 50) + "..." : result.Cookie.Value)}");
                    sb.AppendLine($"  ├─ Статус: {(result.IsValid ? "✓ ВАЛИДНА" : "✗ НЕВАЛИДНА")}");
                    sb.AppendLine($"  ├─ Код ответа: {result.StatusCode}");
                    sb.AppendLine($"  ├─ Время отклика: {result.ResponseTimeMs}ms");
                    sb.AppendLine($"  ├─ Secure: {(result.Cookie.Secure ? "Да" : "Нет")}");
                    sb.AppendLine($"  ├─ HttpOnly: {(result.Cookie.HttpOnly ? "Да" : "Нет")}");
                    sb.AppendLine($"  ├─ Истекла: {(result.Cookie.IsExpired ? "Да" : "Нет")}");
                    sb.AppendLine($"  ├─ Дней до истечения: {result.Cookie.DaysUntilExpiration}");
                    sb.AppendLine($"  ├─ Path: {result.Cookie.Path}");
                    sb.AppendLine($"  └─ SameSite: {result.Cookie.SameSite ?? "Not Set"}");

                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                        sb.AppendLine($"  ⚠️  Ошибка: {result.ErrorMessage}");
                }

                sb.AppendLine("\n────────────────────────────────────────────────────────────────\n");
            }

            // Сводка по доменам
            sb.AppendLine("\n═══════════════════════════════════════════════════════════════════════════════");
            sb.AppendLine("                           СВОДКА ПО ДОМЕНАМ");
            sb.AppendLine("═══════════════════════════════════════════════════════════════════════════════\n");

            foreach (var domainGroup in groupedResults.OrderByDescending(g => g.Count(r => r.IsValid)))
            {
                var domain = domainGroup.Key;
                var validCount = domainGroup.Count(r => r.IsValid);
                var totalCount = domainGroup.Count();
                var percentage = totalCount > 0 ? (validCount * 100) / totalCount : 0;

                var indicator = percentage >= 80 ? "✓" : percentage >= 50 ? "⚠" : "✗";
                sb.AppendLine($"{indicator} {domain,-40} | Валидных: {validCount,3}/{totalCount,3} ({percentage,3}%)");
            }

            sb.AppendLine("\n═══════════════════════════════════════════════════════════════════════════════");
            sb.AppendLine($"Отчёт сгенерирован: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("═══════════════════════════════════════════════════════════════════════════════");

            await File.WriteAllTextAsync(reportPath, sb.ToString(), Encoding.UTF8);
            _logger.Information("Validation report created: {ReportPath}", reportPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error creating validation report");
            throw;
        }
    }

    /// <summary>
    /// Создаёт TXT файл с результатами для каждого сайта
    /// </summary>
    public async Task CreateSiteValidationReportsAsync(
        List<CookieValidationCheckResult> validationResults,
        string outputDirectory)
    {
        try
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var groupedResults = validationResults.GroupBy(r => r.Domain);

            foreach (var domainGroup in groupedResults)
            {
                var domain = domainGroup.Key;
                var domainResults = domainGroup.ToList();

                var fileName = SanitizeFileName($"{domain}_validation_report.txt");
                var filePath = Path.Combine(outputDirectory, fileName);

                var sb = new StringBuilder();
                sb.AppendLine($"╔════════════════════════════════════════════════════════════════╗");
                sb.AppendLine($"║ ОТЧЁТ ПРОВЕРКИ COOKIE ДЛЯ: {domain,-45} ║");
                sb.AppendLine($"╚════════════════════════════════════════════════════════════════╝");
                sb.AppendLine();
                sb.AppendLine($"📅 Дата проверки: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"🌐 Сайт доступен: {(domainResults.First().SiteAccessible ? "ДА" : "НЕТ")}");
                sb.AppendLine($"✓ Всего cookie: {domainResults.Count}");
                sb.AppendLine($"✓ Валидных: {domainResults.Count(r => r.IsValid)}");
                sb.AppendLine($"✗ Невалидных: {domainResults.Count(r => !r.IsValid)}");
                sb.AppendLine();
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine();

                var validCookies = domainResults.Where(r => r.IsValid).ToList();
                if (validCookies.Any())
                {
                    sb.AppendLine("✅ ВАЛИДНЫЕ COOKIE:\n");
                    foreach (var result in validCookies)
                    {
                        sb.AppendLine($"  📌 Имя: {result.Cookie.Name}");
                        sb.AppendLine($"     Статус: {result.StatusCode}");
                        sb.AppendLine($"     Время отклика: {result.ResponseTimeMs}ms");
                        sb.AppendLine($"     Secure: {(result.Cookie.Secure ? "✓" : "✗")} | HttpOnly: {(result.Cookie.HttpOnly ? "✓" : "✗")}");
                        sb.AppendLine();
                    }
                }

                var invalidCookies = domainResults.Where(r => !r.IsValid).ToList();
                if (invalidCookies.Any())
                {
                    sb.AppendLine("\n❌ НЕВАЛИДНЫЕ COOKIE:\n");
                    foreach (var result in invalidCookies)
                    {
                        sb.AppendLine($"  📌 Имя: {result.Cookie.Name}");
                        sb.AppendLine($"     Статус: {result.StatusCode}");
                        sb.AppendLine($"     Причина: {result.ErrorMessage ?? "Неизвестная ошибка"}");
                        sb.AppendLine();
                    }
                }

                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine($"\nЗагенерировано: CookieAnalyzer v1.0");

                await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
                _logger.Information("Site report created for {Domain}: {Path}", domain, filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error creating site validation reports");
            throw;
        }
    }

    /// <summary>
    /// Очищает имя файла от недопустимых символов
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
        return sanitized.Replace(" ", "_").Replace(".", "_").Replace(":", "-");
    }
}
