using System;
using Fundo.Domain.Constants;
using Fundo.Domain.Entities;
using Xunit;

namespace Fundo.Services.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Loan entity business logic.
/// </summary>
public class LoanTests
{
    #region Factory Method Tests (Loan.Create)

    [Fact]
    public void Create_ValidInput_CreatesLoan()
    {
        // Act
        var loan = Loan.Create(10000m, "John Doe");

        // Assert
        Assert.NotEqual(Guid.Empty, loan.Id);
        Assert.Equal(10000m, loan.Amount);
        Assert.Equal(10000m, loan.CurrentBalance);
        Assert.Equal("John Doe", loan.ApplicantName);
        Assert.Equal(LoanStatus.Active, loan.Status);
        Assert.True((DateTimeOffset.UtcNow - loan.CreatedAt).TotalSeconds < 1);
        Assert.True((DateTimeOffset.UtcNow - loan.UpdatedAt).TotalSeconds < 1);
    }

    [Fact]
    public void Create_ZeroAmount_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Loan.Create(0m, "John Doe"));
        Assert.Contains("amount must be greater than zero", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_NegativeAmount_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Loan.Create(-1000m, "John Doe"));
        Assert.Contains("amount must be greater than zero", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_NullApplicantName_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Loan.Create(10000m, null!));
        Assert.Contains("applicant name is required", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_EmptyApplicantName_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Loan.Create(10000m, ""));
        Assert.Contains("applicant name is required", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_WhitespaceApplicantName_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Loan.Create(10000m, "   "));
        Assert.Contains("applicant name is required", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region IsValid Tests

    [Fact]
    public void IsValid_ValidLoan_ReturnsTrue()
    {
        // Arrange
        var loan = new Loan(
            id: Guid.NewGuid(),
            amount: 10000m,
            currentBalance: 5000m,
            applicantName: "John Doe",
            status: LoanStatus.Active,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow
        );

        // Act
        var result = loan.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_BalanceExceedsAmount_ReturnsFalse()
    {
        // Arrange
        var loan = new Loan(
            id: Guid.NewGuid(),
            amount: 10000m,
            currentBalance: 15000m,
            applicantName: "John Doe",
            status: LoanStatus.Active,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow
        );

        // Act
        var result = loan.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_NegativeBalance_ReturnsFalse()
    {
        // Arrange
        var loan = new Loan(
            id: Guid.NewGuid(),
            amount: 10000m,
            currentBalance: -100m,
            applicantName: "John Doe",
            status: LoanStatus.Active,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow
        );

        // Act
        var result = loan.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_BalanceEqualsAmount_ReturnsTrue()
    {
        // Arrange
        var loan = new Loan(
            id: Guid.NewGuid(),
            amount: 10000m,
            currentBalance: 10000m,
            applicantName: "John Doe",
            status: LoanStatus.Active,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow
        );

        // Act
        var result = loan.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ZeroBalance_ReturnsTrue()
    {
        // Arrange
        var loan = new Loan(
            id: Guid.NewGuid(),
            amount: 10000m,
            currentBalance: 0m,
            applicantName: "John Doe",
            status: LoanStatus.Paid,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow
        );

        // Act
        var result = loan.IsValid();

        // Assert
        Assert.True(result);
    }

    #endregion

    #region ApplyPayment Tests

    [Fact]
    public void ApplyPayment_ValidPayment_ReducesBalance()
    {
        // Arrange
        var loan = Loan.Create(10000m, "John Doe");

        // Act
        loan.ApplyPayment(3000m);

        // Assert
        Assert.Equal(7000m, loan.CurrentBalance);
        Assert.Equal(LoanStatus.Active, loan.Status);
    }

    [Fact]
    public void ApplyPayment_PaymentEqualsBalance_SetsStatusToPaid()
    {
        // Arrange
        var loan = Loan.Create(10000m, "John Doe");

        // Act
        loan.ApplyPayment(10000m);

        // Assert
        Assert.Equal(0m, loan.CurrentBalance);
        Assert.Equal(LoanStatus.Paid, loan.Status);
    }

    [Fact]
    public void ApplyPayment_ZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var loan = Loan.Create(10000m, "John Doe");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => loan.ApplyPayment(0m));
        Assert.Contains("payment amount must be greater than zero", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyPayment_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var loan = Loan.Create(10000m, "John Doe");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => loan.ApplyPayment(-500m));
        Assert.Contains("payment amount must be greater than zero", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyPayment_ExceedsBalance_ThrowsInvalidOperationException()
    {
        // Arrange
        var loan = Loan.Create(10000m, "John Doe");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => loan.ApplyPayment(15000m));
        Assert.Contains("cannot exceed current balance", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyPayment_UpdatesTimestamp()
    {
        // Arrange
        var loan = new Loan(
            id: Guid.NewGuid(),
            amount: 10000m,
            currentBalance: 5000m,
            applicantName: "John Doe",
            status: LoanStatus.Active,
            createdAt: DateTimeOffset.UtcNow.AddDays(-10),
            updatedAt: DateTimeOffset.UtcNow.AddDays(-1)
        );
        var beforePayment = loan.UpdatedAt;

        // Act
        loan.ApplyPayment(1000m);

        // Assert
        Assert.True(loan.UpdatedAt > beforePayment);
    }

    #endregion
}
