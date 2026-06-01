using CookieAnalyzer.Core.Models;
using Xunit;
using FluentAssertions;

namespace CookieAnalyzer.Tests;

public class CookieValidationTests
{
    [Fact]
    public void Cookie_Validate_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var cookie = new Cookie
        {
            Domain = "example.com",
            Name = "session",
            ExpirationDate = 1735689600
        };

        // Act
        var result = cookie.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Cookie_Validate_WithMissingDomain_ReturnsFalse()
    {
        // Arrange
        var cookie = new Cookie
        {
            Domain = "",
            Name = "session",
            ExpirationDate = 1735689600
        };

        // Act
        var result = cookie.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.Contains("Domain"));
    }

    [Fact]
    public void Cookie_Validate_WithMissingName_ReturnsFalse()
    {
        // Arrange
        var cookie = new Cookie
        {
            Domain = "example.com",
            Name = "",
            ExpirationDate = 1735689600
        };

        // Act
        var result = cookie.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.Contains("Name"));
    }

    [Fact]
    public void Cookie_IsExpired_WithPastDate_ReturnsTrue()
    {
        // Arrange
        var pastTimestamp = (long)(DateTime.UtcNow.AddDays(-1) - new DateTime(1970, 1, 1)).TotalSeconds;
        var cookie = new Cookie
        {
            Domain = "example.com",
            Name = "session",
            ExpirationDate = pastTimestamp
        };

        // Act & Assert
        cookie.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void Cookie_IsExpired_WithFutureDate_ReturnsFalse()
    {
        // Arrange
        var futureTimestamp = (long)(DateTime.UtcNow.AddDays(30) - new DateTime(1970, 1, 1)).TotalSeconds;
        var cookie = new Cookie
        {
            Domain = "example.com",
            Name = "session",
            ExpirationDate = futureTimestamp
        };

        // Act & Assert
        cookie.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void Cookie_DaysUntilExpiration_CalculatesCorrectly()
    {
        // Arrange
        var futureTimestamp = (long)(DateTime.UtcNow.AddDays(10) - new DateTime(1970, 1, 1)).TotalSeconds;
        var cookie = new Cookie
        {
            Domain = "example.com",
            Name = "session",
            ExpirationDate = futureTimestamp
        };

        // Act & Assert
        cookie.DaysUntilExpiration.Should().Be(10);
    }
}
