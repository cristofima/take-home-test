using Fundo.Application.DTO;
using Fundo.Application.Interfaces;
using Fundo.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Fundo.Application.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly ILogger<LoanService> _logger;

    public LoanService(ILoanRepository loanRepository, ILogger<LoanService> logger)
    {
        _loanRepository = loanRepository;
        _logger = logger;
    }

    public async Task<LoanResponse> CreateLoanAsync(CreateLoanRequest request)
    {
        _logger.LogInformation("Creating new loan for applicant {ApplicantName} with amount {Amount:C}", 
            request.ApplicantName, request.Amount);

        var loan = new Loan
        {
            Amount = request.Amount,
            CurrentBalance = request.Amount, // New loan starts with full balance
            ApplicantName = request.ApplicantName,
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Validate business rules using domain logic
        if (!loan.IsValid())
        {
            _logger.LogWarning("Loan validation failed for applicant {ApplicantName}: Current balance exceeds loan amount", 
                request.ApplicantName);
            throw new InvalidOperationException("Invalid loan data: Current balance exceeds loan amount");
        }

        var createdLoan = await _loanRepository.AddAsync(loan);
        _logger.LogInformation("Loan created successfully with ID {LoanId} for applicant {ApplicantName}", 
            createdLoan.Id, createdLoan.ApplicantName);
        
        return MapToResponse(createdLoan);
    }

    public async Task<LoanResponse?> GetLoanByIdAsync(Guid id)
    {
        _logger.LogDebug("Retrieving loan with ID {LoanId}", id);
        
        var loan = await _loanRepository.GetByIdAsync(id);
        
        if (loan == null)
        {
            _logger.LogWarning("Loan with ID {LoanId} not found", id);
            return null;
        }
        
        return MapToResponse(loan);
    }

    public async Task<IEnumerable<LoanResponse>> GetAllLoansAsync()
    {
        _logger.LogDebug("Retrieving all loans");
        
        var loans = await _loanRepository.GetAllAsync();
        var loansList = loans.ToList();
        
        _logger.LogInformation("Retrieved {LoanCount} loans", loansList.Count);
        
        return loansList.Select(MapToResponse);
    }

    public async Task<LoanResponse> ProcessPaymentAsync(Guid id, PaymentRequest request)
    {
        _logger.LogInformation("Processing payment of {PaymentAmount:C} for loan {LoanId}", 
            request.Amount, id);

        var loan = await _loanRepository.GetByIdAsync(id);
        if (loan == null)
        {
            _logger.LogWarning("Payment failed: Loan with ID {LoanId} not found", id);
            throw new KeyNotFoundException($"Loan with ID {id} not found");
        }

        // Validate payment amount
        if (request.Amount <= 0)
        {
            _logger.LogWarning("Payment validation failed for loan {LoanId}: Invalid amount {Amount}", 
                id, request.Amount);
            throw new ArgumentException("Payment amount must be greater than zero", nameof(request.Amount));
        }

        if (request.Amount > loan.CurrentBalance)
        {
            _logger.LogWarning("Payment validation failed for loan {LoanId}: Payment {PaymentAmount:C} exceeds balance {Balance:C}",
                id, request.Amount, loan.CurrentBalance);
            throw new InvalidOperationException(
                $"Payment amount ${request.Amount:F2} exceeds current balance ${loan.CurrentBalance:F2}");
        }

        var previousBalance = loan.CurrentBalance;
        var previousStatus = loan.Status;

        // Apply payment
        loan.CurrentBalance -= request.Amount;
        
        // Use domain logic to update status and timestamp
        loan.UpdateStatus();

        // Validate business rules after payment
        if (!loan.IsValid())
        {
            _logger.LogError("Payment resulted in invalid loan state for loan {LoanId}", id);
            throw new InvalidOperationException("Payment resulted in invalid loan state");
        }

        await _loanRepository.UpdateAsync(loan);
        
        _logger.LogInformation(
            "Payment processed successfully for loan {LoanId}. Balance: {PreviousBalance:C} -> {NewBalance:C}, Status: {PreviousStatus} -> {NewStatus}",
            id, previousBalance, loan.CurrentBalance, previousStatus, loan.Status);
        
        return MapToResponse(loan);
    }

    private static LoanResponse MapToResponse(Loan loan)
    {
        return new LoanResponse
        {
            Id = loan.Id,
            Amount = loan.Amount,
            CurrentBalance = loan.CurrentBalance,
            ApplicantName = loan.ApplicantName,
            Status = loan.Status,
        };
    }
}
