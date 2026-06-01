namespace CookieAnalyzer.Core.Services;

using System.Collections.Concurrent;
using System.Diagnostics;
using CookieAnalyzer.Core.Models;
using Serilog;

/// <summary>
/// Сервис для проверки валидности cookie на сайтах
/// </summary>
public class CookieValidationService
{
    private readonly ILogger _logger;
    private readonly HttpClientHandler _httpClientHandler;

    public CookieValidationService()
    {
        _logger = Log.ForContext<CookieValidationService>();
        
        _httpClientHandler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new System.Net.CookieContainer(),
            AllowAutoRedirect = true,
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        };
    }

    /// <summary>
    /// Проверяет валидность cookie на сайте путём совершения HTTP запроса
    /// </summary>
    public async Task<CookieValidationCheckResult> ValidateCookieAsync(
        Cookie cookie,
        string domain,
        CookieValidationSettings settings,
        CancellationToken cancellationToken = default)
    {
        var result = new CookieValidationCheckResult
        {
            Cookie = cookie,
            Domain = domain,
            CheckTime = DateTime.UtcNow
        };

        try
        {
            // Пропускаем проверку, если cookie уже истекла
            if (cookie.IsExpired)
            {
                result.IsValid = false;
                result.StatusCode = "410 Gone";
                result.ErrorMessage = "Cookie has expired";
                _logger.Information("Cookie {Name} for {Domain} has expired", cookie.Name, domain);
                return result;
            }

            // Формируем URL для проверки
            var url = FormatDomainToUrl(domain);

            using var httpClient = new HttpClient(_httpClientHandler)
            {
                Timeout = TimeSpan.FromSeconds(settings.RequestTimeoutSeconds)
            };

            // Добавляем cookie в запрос
            var cookieUri = new Uri(url);
            var cookie_net = new System.Net.Cookie(
                cookie.Name,
                cookie.Value,
                cookie.Path,
                ExtractDomainFromUrl(domain))
            {
                Secure = cookie.Secure,
                HttpOnly = cookie.HttpOnly
            };

            _httpClientHandler.CookieContainer.Add(cookieUri, cookie_net);

            // Выполняем HEAD запрос для проверки
            var stopwatch = Stopwatch.StartNew();
            
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            request.Headers.Add("User-Agent", "CookieAnalyzer/1.0 (+https://github.com/dmitrijchis271-lab/cookie-analyzer)");

            var response = await httpClient.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            result.SiteAccessible = true;
            result.StatusCode = $"{(int)response.StatusCode} {response.StatusCode}";

            // Проверяем статус ответа
            // Cookie считается валидной если:
            // - Статус 2xx (успешный ответ)
            // - Статус не 401 (Unauthorized) - нет авторизации
            // - Статус не 403 (Forbidden) - запрет доступа
            result.IsValid = response.IsSuccessStatusCode || 
                           (int)response.StatusCode == 304 ||
                           (int)response.StatusCode == 404;

            if (!result.IsValid && !response.IsSuccessStatusCode)
            {
                result.ErrorMessage = $"Server responded with {response.StatusCode}";
            }

            _logger.Information(
                "Cookie validation for {Domain}: {IsValid} (Status: {StatusCode}, ResponseTime: {ResponseTime}ms)",
                domain,
                result.IsValid,
                result.StatusCode,
                result.ResponseTimeMs);
        }
        catch (HttpRequestException ex)
        {
            result.SiteAccessible = false;
            result.IsValid = false;
            result.StatusCode = "Connection Error";
            result.ErrorMessage = $"Cannot connect to {domain}: {ex.Message}";
            _logger.Warning(ex, "Cannot connect to {Domain}", domain);
        }
        catch (TaskCanceledException ex)
        {
            result.SiteAccessible = false;
            result.IsValid = false;
            result.StatusCode = "Timeout";
            result.ErrorMessage = $"Request timeout for {domain}";
            _logger.Warning(ex, "Request timeout for {Domain}", domain);
        }
        catch (Exception ex)
        {
            result.SiteAccessible = false;
            result.IsValid = false;
            result.StatusCode = "Error";
            result.ErrorMessage = ex.Message;
            _logger.Error(ex, "Error validating cookie for {Domain}", domain);
        }

        return result;
    }

    /// <summary>
    /// Проверяет множество cookie асинхронно с ограничением одновременных запросов
    /// </summary>
    public async Task<List<CookieValidationCheckResult>> ValidateCookiesBatchAsync(
        List<(Cookie cookie, string domain)> cookiesToValidate,
        CookieValidationSettings settings,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var results = new ConcurrentBag<CookieValidationCheckResult>();
        var semaphore = new SemaphoreSlim(settings.MaxConcurrentRequests);

        try
        {
            var tasks = cookiesToValidate.Select(async item =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    progress?.Report($"Проверка: {item.domain}/{item.cookie.Name}");
                    var result = await ValidateCookieAsync(item.cookie, item.domain, settings, cancellationToken);
                    results.Add(result);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            _logger.Information("Batch validation completed. Total: {Total}, Valid: {Valid}",
                results.Count,
                results.Count(r => r.IsValid));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in batch validation");
        }
        finally
        {
            semaphore.Dispose();
        }

        return results.ToList();
    }

    /// <summary>
    /// Проверяет доступность сайта
    /// </summary>
    public async Task<bool> CheckSiteAvailabilityAsync(
        string domain,
        int timeoutSeconds = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = FormatDomainToUrl(domain);
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(timeoutSeconds) };
            
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            request.Headers.Add("User-Agent", "CookieAnalyzer/1.0");
            
            var response = await httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode || (int)response.StatusCode < 500;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Site {Domain} is not accessible", domain);
            return false;
        }
    }

    /// <summary>
    /// Форматирует домен в полный URL
    /// </summary>
    private static string FormatDomainToUrl(string domain)
    {
        var cleanDomain = domain.TrimStart('.');
        
        if (cleanDomain.StartsWith("http://") || cleanDomain.StartsWith("https://"))
            return cleanDomain;

        return $"https://{cleanDomain}";
    }

    /// <summary>
    /// Извлекает домен из URL
    /// </summary>
    private static string ExtractDomainFromUrl(string domain)
    {
        var cleanDomain = domain.TrimStart('.');
        
        if (cleanDomain.StartsWith("http://"))
            cleanDomain = cleanDomain.Substring(7);
        else if (cleanDomain.StartsWith("https://"))
            cleanDomain = cleanDomain.Substring(8);

        return cleanDomain.Split('/')[0];
    }
}
