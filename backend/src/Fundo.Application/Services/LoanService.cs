using Fundo.Application.DTO;
using Fundo.Application.Interfaces;
using Fundo.Domain.Entities;

namespace Fundo.Application.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;

    public LoanService(ILoanRepository loanRepository)
    {
        _loanRepository = loanRepository;
    }

    public async Task<LoanResponse> CreateLoanAsync(CreateLoanRequest request)
    {
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
            throw new InvalidOperationException("Invalid loan data: Current balance exceeds loan amount");
        }

        var createdLoan = await _loanRepository.AddAsync(loan);
        return MapToResponse(createdLoan);
    }

    public async Task<LoanResponse?> GetLoanByIdAsync(Guid id)
    {
        var loan = await _loanRepository.GetByIdAsync(id);
        return loan == null ? null : MapToResponse(loan);
    }

    public async Task<IEnumerable<LoanResponse>> GetAllLoansAsync()
    {
        var loans = await _loanRepository.GetAllAsync();
        return loans.Select(MapToResponse);
    }

    public async Task<LoanResponse> ProcessPaymentAsync(Guid id, PaymentRequest request)
    {
        var loan = await _loanRepository.GetByIdAsync(id);
        if (loan == null)
        {
            throw new KeyNotFoundException($"Loan with ID {id} not found");
        }

        // Validate payment amount
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Payment amount must be greater than zero", nameof(request.Amount));
        }

        if (request.Amount > loan.CurrentBalance)
        {
            throw new InvalidOperationException(
                $"Payment amount ${request.Amount:F2} exceeds current balance ${loan.CurrentBalance:F2}");
        }

        // Apply payment
        loan.CurrentBalance -= request.Amount;
        
        // Use domain logic to update status and timestamp
        loan.UpdateStatus();

        // Validate business rules after payment
        if (!loan.IsValid())
        {
            throw new InvalidOperationException("Payment resulted in invalid loan state");
        }

        await _loanRepository.UpdateAsync(loan);
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
