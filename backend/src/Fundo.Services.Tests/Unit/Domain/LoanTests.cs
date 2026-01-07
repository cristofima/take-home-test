using System;
using Fundo.Domain.Entities;
using Xunit;

namespace Fundo.Services.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Loan entity business logic.
/// </summary>
public class LoanTests
{
    [Fact]
    public void IsValid_ValidLoan_ReturnsTrue()
    {
        // Arrange
        var loan = new Loan
        {
            Amount = 10000m,
            CurrentBalance = 5000m
        };

        // Act
        var result = loan.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_BalanceExceedsAmount_ReturnsFalse()
    {
        // Arrange
        var loan = new Loan
        {
            Amount = 10000m,
            CurrentBalance = 15000m
        };

        // Act
        var result = loan.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_NegativeBalance_ReturnsFalse()
    {
        // Arrange
        var loan = new Loan
        {
            Amount = 10000m,
            CurrentBalance = -100m
        };

        // Act
        var result = loan.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_BalanceEqualsAmount_ReturnsTrue()
    {
        // Arrange
        var loan = new Loan
        {
            Amount = 10000m,
            CurrentBalance = 10000m
        };

        // Act
        var result = loan.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ZeroBalance_ReturnsTrue()
    {
        // Arrange
        var loan = new Loan
        {
            Amount = 10000m,
            CurrentBalance = 0m
        };

        // Act
        var result = loan.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void UpdateStatus_BalanceZero_SetsStatusToPaid()
    {
        // Arrange
        var loan = new Loan
        {
            Amount = 10000m,
            CurrentBalance = 0m,
            Status = "active",
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };
        var beforeUpdate = loan.UpdatedAt;

        // Act
        loan.UpdateStatus();

        // Assert
        Assert.Equal("paid", loan.Status);
        Assert.True(loan.UpdatedAt > beforeUpdate);
    }

    [Fact]
    public void UpdateStatus_NegativeBalance_SetsStatusToPaid()
    {
        // Arrange
        var loan = new Loan
        {
            Amount = 10000m,
            CurrentBalance = -100m,
            Status = "active"
        };

        // Act
        loan.UpdateStatus();

        // Assert
        Assert.Equal("paid", loan.Status);
    }

    [Fact]
    public void UpdateStatus_BalanceGreaterThanZero_KeepsStatusUnchanged()
    {
        // Arrange
        var loan = new Loan
        {
            Amount = 10000m,
            CurrentBalance = 5000m,
            Status = "active"
        };

        // Act
        loan.UpdateStatus();

        // Assert
        Assert.Equal("active", loan.Status);
    }

    [Fact]
    public void UpdateStatus_AlwaysUpdatesTimestamp()
    {
        // Arrange
        var loan = new Loan
        {
            Amount = 10000m,
            CurrentBalance = 5000m,
            Status = "active",
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };
        var beforeUpdate = loan.UpdatedAt;

        // Act
        loan.UpdateStatus();

        // Assert
        Assert.True(loan.UpdatedAt > beforeUpdate);
    }
}
