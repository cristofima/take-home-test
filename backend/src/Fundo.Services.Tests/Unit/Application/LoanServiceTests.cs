using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fundo.Application.DTO;
using Fundo.Application.Services;
using Fundo.Domain.Constants;
using Fundo.Domain.Entities;
using Fundo.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Fundo.Services.Tests.Unit.Application;

/// <summary>
/// Unit tests for LoanService business logic.
/// </summary>
public class LoanServiceTests
{
    private readonly Mock<ILoanRepository> _repositoryMock;
    private readonly Mock<ILogger<LoanService>> _loggerMock;
    private readonly LoanService _service;

    public LoanServiceTests()
    {
        _repositoryMock = new Mock<ILoanRepository>();
        _loggerMock = new Mock<ILogger<LoanService>>();
        _service = new LoanService(_repositoryMock.Object, _loggerMock.Object);
    }

    #region CreateLoanAsync Tests

    [Fact]
    public async Task CreateLoanAsync_ValidRequest_CreatesLoan()
    {
        // Arrange
        var request = new CreateLoanRequest
        {
            Amount = 10000m,
            ApplicantName = "John Doe"
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Loan>()))
            .ReturnsAsync((Loan loan) => loan);

        // Act
        var result = await _service.CreateLoanAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Amount, result.Amount);
        Assert.Equal(request.Amount, result.CurrentBalance);
        Assert.Equal(request.ApplicantName, result.ApplicantName);
        Assert.Equal("active", result.Status);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Loan>()), Times.Once);
    }

    [Fact]
    public async Task CreateLoanAsync_SetsInitialBalanceToAmount()
    {
        // Arrange
        var request = new CreateLoanRequest
        {
            Amount = 15000m,
            ApplicantName = "Jane Smith"
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Loan>()))
            .ReturnsAsync((Loan loan) => loan);

        // Act
        var result = await _service.CreateLoanAsync(request);

        // Assert
        Assert.Equal(result.Amount, result.CurrentBalance);
    }

    #endregion

    #region GetLoanByIdAsync Tests

    [Fact]
    public async Task GetLoanByIdAsync_ExistingLoan_ReturnsLoan()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var loan = new Loan(
            id: loanId,
            amount: 10000m,
            currentBalance: 5000m,
            applicantName: "John Doe",
            status: LoanStatus.Active,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow
        );

        _repositoryMock
            .Setup(r => r.GetByIdAsync(loanId))
            .ReturnsAsync(loan);

        // Act
        var result = await _service.GetLoanByIdAsync(loanId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(loanId, result.Id);
        Assert.Equal(loan.Amount, result.Amount);
        Assert.Equal(loan.CurrentBalance, result.CurrentBalance);
    }

    [Fact]
    public async Task GetLoanByIdAsync_NonExistentLoan_ReturnsNull()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(loanId))
            .ReturnsAsync((Loan?)null);

        // Act
        var result = await _service.GetLoanByIdAsync(loanId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetAllLoansAsync Tests

    [Fact]
    public async Task GetAllLoansAsync_ReturnsAllLoans()
    {
        // Arrange
        var loans = new List<Loan>
        {
            new(Guid.NewGuid(), 10000m, 10000m, "John", LoanStatus.Active, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), 15000m, 0m, "Jane", LoanStatus.Paid, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), 20000m, 15000m, "Bob", LoanStatus.Active, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(loans);

        // Act
        var result = await _service.GetAllLoansAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAllLoansAsync_EmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Loan>());

        // Act
        var result = await _service.GetAllLoansAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region ProcessPaymentAsync Tests

    [Fact]
    public async Task ProcessPaymentAsync_ValidPayment_UpdatesBalance()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var loan = new Loan(
            id: loanId,
            amount: 10000m,
            currentBalance: 10000m,
            applicantName: "John Doe",
            status: LoanStatus.Active,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow
        );

        var paymentRequest = new PaymentRequest { Amount = 3000m };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(loanId))
            .ReturnsAsync(loan);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Loan>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ProcessPaymentAsync(loanId, paymentRequest);

        // Assert
        Assert.Equal(7000m, result.CurrentBalance);
        Assert.Equal(LoanStatus.Active, result.Status);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Loan>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentAsync_FullPayment_SetsStatusToPaid()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var loan = new Loan(
            id: loanId,
            amount: 10000m,
            currentBalance: 5000m,
            applicantName: "John Doe",
            status: LoanStatus.Active,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow
        );

        var paymentRequest = new PaymentRequest { Amount = 5000m };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(loanId))
            .ReturnsAsync(loan);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Loan>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ProcessPaymentAsync(loanId, paymentRequest);

        // Assert
        Assert.Equal(0m, result.CurrentBalance);
        Assert.Equal(LoanStatus.Paid, result.Status);
    }

    [Fact]
    public async Task ProcessPaymentAsync_NonExistentLoan_ThrowsKeyNotFoundException()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var paymentRequest = new PaymentRequest { Amount = 1000m };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(loanId))
            .ReturnsAsync((Loan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.ProcessPaymentAsync(loanId, paymentRequest));
    }

    [Fact]
    public async Task ProcessPaymentAsync_ZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var loan = new Loan(
            id: loanId,
            amount: 10000m,
            currentBalance: 5000m,
            applicantName: "John Doe",
            status: LoanStatus.Active,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow
        );

        var paymentRequest = new PaymentRequest { Amount = 0m };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(loanId))
            .ReturnsAsync(loan);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ProcessPaymentAsync(loanId, paymentRequest));
    }

    [Fact]
    public async Task ProcessPaymentAsync_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var loan = new Loan(
            id: loanId,
            amount: 10000m,
            currentBalance: 5000m,
            applicantName: "John Doe",
            status: LoanStatus.Active,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow
        );

        var paymentRequest = new PaymentRequest { Amount = -100m };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(loanId))
            .ReturnsAsync(loan);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ProcessPaymentAsync(loanId, paymentRequest));
    }

    [Fact]
    public async Task ProcessPaymentAsync_ExceedsBalance_ThrowsInvalidOperationException()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var loan = new Loan(
            id: loanId,
            amount: 10000m,
            currentBalance: 3000m,
            applicantName: "John Doe",
            status: LoanStatus.Active,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow
        );

        var paymentRequest = new PaymentRequest { Amount = 5000m };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(loanId))
            .ReturnsAsync(loan);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ProcessPaymentAsync(loanId, paymentRequest));
    }

    #endregion
}
