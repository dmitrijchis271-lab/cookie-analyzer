using CookieAnalyzer.Core.Models;
using CookieAnalyzer.Core.Services;
using Xunit;
using FluentAssertions;

namespace CookieAnalyzer.Tests;

public class ExportServiceTests
{
    private readonly ExportService _service;
    private readonly string _testOutputDirectory;

    public ExportServiceTests()
    {
        _service = new ExportService();
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), $"export_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testOutputDirectory);
    }

    [Fact]
    public async Task ExportResultsAsync_ExportsCsvFile()
    {
        // Arrange
        var results = new List<CookieAnalysisResult>
        {
            new()
            {
                FilePath = "test.json",
                IsSuccessful = true,
                Cookies = new List<Cookie>
                {
                    new()
                    {
                        Domain = "example.com",
                        Name = "session",
                        ExpirationDate = 1735689600,
                        Secure = true,
                        HttpOnly = true
                    }
                }
            }
        };

        var settings = new ExportSettings
        {
            OutputDirectory = _testOutputDirectory,
            ExportCsv = true,
            ExportJson = false,
            OutputFileName = "test_report"
        };

        // Act
        await _service.ExportResultsAsync(results, settings);

        // Assert
        var csvFile = Path.Combine(_testOutputDirectory, "test_report.csv");
        File.Exists(csvFile).Should().BeTrue();
    }

    [Fact]
    public async Task ExportResultsAsync_ExportsJsonFile()
    {
        // Arrange
        var results = new List<CookieAnalysisResult>
        {
            new()
            {
                FilePath = "test.json",
                IsSuccessful = true,
                Cookies = new List<Cookie>
                {
                    new()
                    {
                        Domain = "example.com",
                        Name = "session",
                        ExpirationDate = 1735689600
                    }
                }
            }
        };

        var settings = new ExportSettings
        {
            OutputDirectory = _testOutputDirectory,
            ExportCsv = false,
            ExportJson = true,
            OutputFileName = "test_report"
        };

        // Act
        await _service.ExportResultsAsync(results, settings);

        // Assert
        var jsonFile = Path.Combine(_testOutputDirectory, "test_report.json");
        File.Exists(jsonFile).Should().BeTrue();
    }

    [Fact]
    public async Task ExportResultsAsync_CreatesOutputDirectory_IfNotExists()
    {
        // Arrange
        var newDir = Path.Combine(_testOutputDirectory, "new_export_dir");
        var results = new List<CookieAnalysisResult>();
        
        var settings = new ExportSettings
        {
            OutputDirectory = newDir,
            ExportJson = false,
            ExportCsv = false
        };

        // Act
        await _service.ExportResultsAsync(results, settings);

        // Assert
        Directory.Exists(newDir).Should().BeTrue();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testOutputDirectory))
            Directory.Delete(_testOutputDirectory, true);
    }
}
