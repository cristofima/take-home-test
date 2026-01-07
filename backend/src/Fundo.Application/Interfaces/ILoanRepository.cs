using Fundo.Domain.Entities;

namespace Fundo.Application.Interfaces;

/// <summary>
/// Repository interface for data access operations
/// </summary>
public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id);
    Task<IEnumerable<Loan>> GetAllAsync();
    Task<Loan> AddAsync(Loan loan);
    Task UpdateAsync(Loan loan);
    Task DeleteAsync(Guid id);
}