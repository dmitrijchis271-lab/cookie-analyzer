using CookieAnalyzer.Core.Models;
using CookieAnalyzer.Core.Services;
using Xunit;
using FluentAssertions;

namespace CookieAnalyzer.Tests;

public class CookieReaderServiceTests
{
    private readonly CookieReaderService _service;

    public CookieReaderServiceTests()
    {
        _service = new CookieReaderService();
    }

    [Fact]
    public async Task ReadCookiesFromFileAsync_WithValidJsonFile_ReturnsCookies()
    {
        // Arrange
        var testFile = CreateTestJsonFile(new List<Cookie>
        {
            new()
            {
                Domain = "example.com",
                Name = "session",
                Value = "abc123",
                ExpirationDate = 1735689600,
                Secure = true,
                HttpOnly = true
            }
        });

        try
        {
            // Act
            var result = await _service.ReadCookiesFromFileAsync(testFile);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Cookies.Should().HaveCount(1);
            result.Cookies[0].Domain.Should().Be("example.com");
            result.Cookies[0].Name.Should().Be("session");
        }
        finally
        {
            if (File.Exists(testFile))
                File.Delete(testFile);
        }
    }

    [Fact]
    public async Task ReadCookiesFromFileAsync_WithNonExistentFile_ReturnsError()
    {
        // Act
        var result = await _service.ReadCookiesFromFileAsync("/nonexistent/file.json");

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Errors.Should().Contain(x => x.Contains("File not found"));
    }

    [Fact]
    public async Task ReadCookiesFromFileAsync_WithInvalidJson_ReturnsError()
    {
        // Arrange
        var testFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(testFile, "invalid json content {");

        try
        {
            // Act
            var result = await _service.ReadCookiesFromFileAsync(testFile);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }
        finally
        {
            if (File.Exists(testFile))
                File.Delete(testFile);
        }
    }

    [Fact]
    public async Task ReadCookiesFromFileAsync_ValidatesCookieData()
    {
        // Arrange
        var testFile = CreateTestJsonFile(new List<Cookie>
        {
            new()
            {
                Domain = "",  // Invalid - empty domain
                Name = "test",
                ExpirationDate = 1735689600
            }
        });

        try
        {
            // Act
            var result = await _service.ReadCookiesFromFileAsync(testFile);

            // Assert
            result.Cookies.Should().BeEmpty();
            result.Errors.Should().NotBeEmpty();
        }
        finally
        {
            if (File.Exists(testFile))
                File.Delete(testFile);
        }
    }

    private string CreateTestJsonFile(List<Cookie> cookies)
    {
        var testFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        var json = System.Text.Json.JsonSerializer.Serialize(cookies, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(testFile, json);
        return testFile;
    }
}
