using Fundo.Application.DTO;
using Fundo.Application.Interfaces;
using Fundo.Application.Utils;
using Fundo.Domain.Entities;
using Fundo.Domain.Interfaces;
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

        // Use domain factory method - validation and business rules handled by domain
        var loan = Loan.Create(request.Amount, request.ApplicantName);

        var createdLoan = await _loanRepository.AddAsync(loan);
        _logger.LogInformation("Loan created successfully with ID {LoanId} for applicant {ApplicantName}", 
            createdLoan.Id, createdLoan.ApplicantName);
        
        return LoanUtils.MapToResponse(createdLoan);
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
        
        return LoanUtils.MapToResponse(loan);
    }

    public async Task<IEnumerable<LoanResponse>> GetAllLoansAsync()
    {
        _logger.LogDebug("Retrieving all loans");
        
        var loans = await _loanRepository.GetAllAsync();
        var loanResponses = loans.Select(LoanUtils.MapToResponse).ToList();
        
        _logger.LogInformation("Retrieved {LoanCount} loans", loanResponses.Count);
        
        return loanResponses;
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

        var previousBalance = loan.CurrentBalance;
        var previousStatus = loan.Status;

        // Use domain method - validation and business rules handled by domain
        loan.ApplyPayment(request.Amount);

        await _loanRepository.UpdateAsync(loan);
        
        _logger.LogInformation(
            "Payment processed successfully for loan {LoanId}. Balance: {PreviousBalance:C} -> {NewBalance:C}, Status: {PreviousStatus} -> {NewStatus}",
            id, previousBalance, loan.CurrentBalance, previousStatus, loan.Status);
        
        return LoanUtils.MapToResponse(loan);
    }
}
