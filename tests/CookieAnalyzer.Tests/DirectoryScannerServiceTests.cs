using CookieAnalyzer.Core.Models;
using CookieAnalyzer.Core.Services;
using Xunit;
using FluentAssertions;

namespace CookieAnalyzer.Tests;

public class DirectoryScannerServiceTests
{
    private readonly DirectoryScannerService _service;
    private readonly string _testDirectory;

    public DirectoryScannerServiceTests()
    {
        _service = new DirectoryScannerService(new CookieReaderService());
        _testDirectory = Path.Combine(Path.GetTempPath(), $"cookie_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public async Task ScanDirectoryAsync_WithJsonFiles_FindsAndReadsFiles()
    {
        // Arrange
        CreateTestJsonFile("cookies1.json", new List<Cookie>
        {
            new()
            {
                Domain = "example1.com",
                Name = "session1",
                ExpirationDate = 1735689600
            }
        });

        CreateTestJsonFile("cookies2.json", new List<Cookie>
        {
            new()
            {
                Domain = "example2.com",
                Name = "session2",
                ExpirationDate = 1735689600
            }
        });

        // Act
        var results = await _service.ScanDirectoryAsync(_testDirectory);

        // Assert
        results.Should().NotBeEmpty();
        var totalCookies = results.Sum(r => r.Cookies.Count);
        totalCookies.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task ScanDirectoryAsync_WithNonExistentDirectory_ReturnsEmpty()
    {
        // Act
        var results = await _service.ScanDirectoryAsync("/nonexistent/path");

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task ScanDirectoryAsync_ScandsRecursively_FindsNestedFiles()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "subdir");
        Directory.CreateDirectory(subDir);
        
        CreateTestJsonFile(Path.Combine(subDir, "cookies.json"), new List<Cookie>
        {
            new()
            {
                Domain = "nested.com",
                Name = "test",
                ExpirationDate = 1735689600
            }
        });

        // Act
        var results = await _service.ScanDirectoryAsync(_testDirectory);

        // Assert
        results.Should().NotBeEmpty();
    }

    private void CreateTestJsonFile(string fileName, List<Cookie> cookies)
    {
        var filePath = Path.Combine(_testDirectory, fileName);
        var json = System.Text.Json.JsonSerializer.Serialize(cookies);
        File.WriteAllText(filePath, json);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }
}
