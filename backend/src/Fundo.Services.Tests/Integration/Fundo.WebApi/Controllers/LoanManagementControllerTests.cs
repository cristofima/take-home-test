using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Fundo.Application.DTO;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Fundo.Services.Tests.Integration.Fundo.WebApi.Controllers;

/// <summary>
/// Integration tests for LoanManagementController API endpoints.
/// </summary>
public class LoanManagementControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LoanManagementControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region GET /api/loans Tests

    [Fact]
    public async Task GetAllLoans_ReturnsOkStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/loans");

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAllLoans_ReturnsCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/loans");

        // Assert
        Assert.Equal("application/json; charset=utf-8",
            response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task GetAllLoans_ReturnsListOfLoans()
    {
        // Act
        var response = await _client.GetAsync("/api/loans");
        var loans = await response.Content.ReadFromJsonAsync<List<LoanResponse>>();

        // Assert
        Assert.NotNull(loans);
        Assert.NotEmpty(loans); // Database is seeded with test data
        Assert.All(loans, loan =>
        {
            Assert.NotEqual(Guid.Empty, loan.Id);
            Assert.True(loan.Amount > 0);
            Assert.True(loan.CurrentBalance >= 0);
            Assert.False(string.IsNullOrWhiteSpace(loan.ApplicantName));
            Assert.False(string.IsNullOrWhiteSpace(loan.Status));
        });
    }

    #endregion

    #region GET /api/loans/{id} Tests

    [Fact]
    public async Task GetLoanById_ExistingId_ReturnsOkWithLoan()
    {
        // Arrange - Get an existing loan ID from the seeded data
        var allLoansResponse = await _client.GetAsync("/api/loans");
        var allLoans = await allLoansResponse.Content.ReadFromJsonAsync<List<LoanResponse>>();
        var existingLoanId = allLoans!.First().Id;

        // Act
        var response = await _client.GetAsync($"/api/loans/{existingLoanId}");
        var loan = await response.Content.ReadFromJsonAsync<LoanResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(loan);
        Assert.Equal(existingLoanId, loan.Id);
        Assert.True(loan.Amount > 0);
        Assert.False(string.IsNullOrWhiteSpace(loan.ApplicantName));
    }

    [Fact]
    public async Task GetLoanById_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/loans/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("not found", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetLoanById_InvalidGuidFormat_ReturnsNotFound()
    {
        // Act - ASP.NET Core routing treats invalid GUID format as no route match
        var response = await _client.GetAsync("/api/loans/invalid-guid");

        // Assert - Returns 404 NotFound because route constraint {id:guid} doesn't match
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region POST /api/loans Tests

    [Fact]
    public async Task CreateLoan_ValidRequest_ReturnsCreatedWithLocation()
    {
        // Arrange
        var request = new CreateLoanRequest
        {
            Amount = 10000.00m,
            ApplicantName = "John Doe"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/loans", request);
        var createdLoan = await response.Content.ReadFromJsonAsync<LoanResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(createdLoan);
        Assert.NotEqual(Guid.Empty, createdLoan.Id);
        Assert.Equal(request.Amount, createdLoan.Amount);
        Assert.Equal(request.ApplicantName, createdLoan.ApplicantName);
        Assert.Equal(request.Amount, createdLoan.CurrentBalance); // Initial balance equals amount
        Assert.Equal("active", createdLoan.Status); // Default status is "active"

        // Verify Location header
        Assert.NotNull(response.Headers.Location);
        Assert.Contains($"/api/loans/{createdLoan.Id}", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task CreateLoan_NegativeAmount_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateLoanRequest
        {
            Amount = -1000.00m,
            ApplicantName = "Jane Smith"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/loans", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLoan_ZeroAmount_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateLoanRequest
        {
            Amount = 0m,
            ApplicantName = "Jane Smith"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/loans", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLoan_ExcessiveAmount_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateLoanRequest
        {
            Amount = 2000000.00m, // Exceeds $1,000,000 limit
            ApplicantName = "Rich Person"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/loans", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLoan_EmptyApplicantName_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateLoanRequest
        {
            Amount = 5000.00m,
            ApplicantName = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/loans", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLoan_ApplicantNameTooShort_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateLoanRequest
        {
            Amount = 5000.00m,
            ApplicantName = "A" // Less than 2 characters
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/loans", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLoan_ApplicantNameTooLong_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateLoanRequest
        {
            Amount = 5000.00m,
            ApplicantName = new string('A', 101) // More than 100 characters
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/loans", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region POST /api/loans/{id}/payment Tests

    [Fact]
    public async Task ProcessPayment_ValidAmount_UpdatesBalanceCorrectly()
    {
        // Arrange - Create a loan first
        var createRequest = new CreateLoanRequest
        {
            Amount = 10000.00m,
            ApplicantName = "Payment Test User"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/loans", createRequest);
        var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanResponse>();

        var paymentRequest = new PaymentRequest
        {
            Amount = 3000.00m
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/loans/{createdLoan!.Id}/payment",
            paymentRequest);
        var updatedLoan = await response.Content.ReadFromJsonAsync<LoanResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updatedLoan);
        Assert.Equal(7000.00m, updatedLoan.CurrentBalance); // 10000 - 3000
        Assert.Equal("active", updatedLoan.Status); // Still active since balance > 0
    }

    [Fact]
    public async Task ProcessPayment_PaymentEqualsBalance_SetsStatusToPaid()
    {
        // Arrange - Create a loan
        var createRequest = new CreateLoanRequest
        {
            Amount = 5000.00m,
            ApplicantName = "Full Payment User"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/loans", createRequest);
        var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanResponse>();

        var paymentRequest = new PaymentRequest
        {
            Amount = 5000.00m // Pay full amount
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/loans/{createdLoan!.Id}/payment",
            paymentRequest);
        var updatedLoan = await response.Content.ReadFromJsonAsync<LoanResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updatedLoan);
        Assert.Equal(0m, updatedLoan.CurrentBalance);
        Assert.Equal("paid", updatedLoan.Status); // Status automatically updated by domain logic
    }

    [Fact]
    public async Task ProcessPayment_MultiplePayments_UpdatesBalanceCorrectly()
    {
        // Arrange - Create a loan
        var createRequest = new CreateLoanRequest
        {
            Amount = 10000.00m,
            ApplicantName = "Multiple Payments User"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/loans", createRequest);
        var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanResponse>();

        // Act - Make multiple payments
        await _client.PostAsJsonAsync(
            $"/api/loans/{createdLoan!.Id}/payment",
            new PaymentRequest { Amount = 3000.00m });

        await _client.PostAsJsonAsync(
            $"/api/loans/{createdLoan.Id}/payment",
            new PaymentRequest { Amount = 2000.00m });

        var finalResponse = await _client.PostAsJsonAsync(
            $"/api/loans/{createdLoan.Id}/payment",
            new PaymentRequest { Amount = 5000.00m });
        var finalLoan = await finalResponse.Content.ReadFromJsonAsync<LoanResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, finalResponse.StatusCode);
        Assert.NotNull(finalLoan);
        Assert.Equal(0m, finalLoan.CurrentBalance); // 10000 - 3000 - 2000 - 5000
        Assert.Equal("paid", finalLoan.Status);
    }

    [Fact]
    public async Task ProcessPayment_ExcessiveAmount_ReturnsBadRequest()
    {
        // Arrange - Create a loan
        var createRequest = new CreateLoanRequest
        {
            Amount = 5000.00m,
            ApplicantName = "Overpayment User"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/loans", createRequest);
        var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanResponse>();

        var paymentRequest = new PaymentRequest
        {
            Amount = 6000.00m // More than balance
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/loans/{createdLoan!.Id}/payment",
            paymentRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("exceed", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcessPayment_NegativeAmount_ReturnsBadRequest()
    {
        // Arrange - Get an existing loan
        var allLoansResponse = await _client.GetAsync("/api/loans");
        var allLoans = await allLoansResponse.Content.ReadFromJsonAsync<List<LoanResponse>>();
        var existingLoanId = allLoans!.First().Id;

        var paymentRequest = new PaymentRequest
        {
            Amount = -100.00m
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/loans/{existingLoanId}/payment",
            paymentRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProcessPayment_ZeroAmount_ReturnsBadRequest()
    {
        // Arrange - Get an existing loan
        var allLoansResponse = await _client.GetAsync("/api/loans");
        var allLoans = await allLoansResponse.Content.ReadFromJsonAsync<List<LoanResponse>>();
        var existingLoanId = allLoans!.First().Id;

        var paymentRequest = new PaymentRequest
        {
            Amount = 0m
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/loans/{existingLoanId}/payment",
            paymentRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProcessPayment_NonExistentLoan_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var paymentRequest = new PaymentRequest
        {
            Amount = 100.00m
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/loans/{nonExistentId}/payment",
            paymentRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ProcessPayment_AlreadyPaidLoan_ReturnsBadRequest()
    {
        // Arrange - Create a loan and pay it off completely
        var createRequest = new CreateLoanRequest
        {
            Amount = 1000.00m,
            ApplicantName = "Already Paid User"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/loans", createRequest);
        var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanResponse>();

        // Pay off the loan completely
        await _client.PostAsJsonAsync(
            $"/api/loans/{createdLoan!.Id}/payment",
            new PaymentRequest { Amount = 1000.00m });

        // Attempt to make another payment
        var paymentRequest = new PaymentRequest
        {
            Amount = 100.00m
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/loans/{createdLoan.Id}/payment",
            paymentRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Integration Tests Across Multiple Operations

    [Fact]
    public async Task LoanLifecycle_CreateRetrievePayRetrieve_WorksCorrectly()
    {
        // Step 1: Create a loan
        var createRequest = new CreateLoanRequest
        {
            Amount = 15000.00m,
            ApplicantName = "Lifecycle Test User"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/loans", createRequest);
        var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanResponse>();
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // Step 2: Retrieve the loan
        var getResponse = await _client.GetAsync($"/api/loans/{createdLoan!.Id}");
        var retrievedLoan = await getResponse.Content.ReadFromJsonAsync<LoanResponse>();
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(createdLoan.Amount, retrievedLoan!.Amount);

        // Step 3: Make a payment
        var paymentResponse = await _client.PostAsJsonAsync(
            $"/api/loans/{createdLoan.Id}/payment",
            new PaymentRequest { Amount = 5000.00m });
        var updatedLoan = await paymentResponse.Content.ReadFromJsonAsync<LoanResponse>();
        Assert.Equal(HttpStatusCode.OK, paymentResponse.StatusCode);
        Assert.Equal(10000.00m, updatedLoan!.CurrentBalance);

        // Step 4: Verify the loan appears in the list with updated balance
        var listResponse = await _client.GetAsync("/api/loans");
        var allLoans = await listResponse.Content.ReadFromJsonAsync<List<LoanResponse>>();
        var loanInList = allLoans!.FirstOrDefault(l => l.Id == createdLoan.Id);
        Assert.NotNull(loanInList);
        Assert.Equal(10000.00m, loanInList.CurrentBalance);
    }

    #endregion
}