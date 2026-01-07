using Fundo.Application.DTO;

namespace Fundo.Application.Interfaces;

/// <summary>
/// Service interface for loan management business logic
/// </summary>
public interface ILoanService
{
    /// <summary>
    /// Creates a new loan
    /// </summary>
    Task<LoanResponse> CreateLoanAsync(CreateLoanRequest request);

    /// <summary>
    /// Retrieves a loan by ID
    /// </summary>
    Task<LoanResponse?> GetLoanByIdAsync(Guid id);

    /// <summary>
    /// Retrieves all loans
    /// </summary>
    Task<IEnumerable<LoanResponse>> GetAllLoansAsync();

    /// <summary>
    /// Processes a payment against a loan, reducing the current balance
    /// </summary>
    Task<LoanResponse> ProcessPaymentAsync(Guid id, PaymentRequest request);
}
