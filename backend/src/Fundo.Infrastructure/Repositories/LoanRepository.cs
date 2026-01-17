using Fundo.Application.Interfaces;
using Fundo.Domain.Entities;
using Fundo.Domain.Interfaces;
using Fundo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Infrastructure.Repositories;

public class LoanRepository : ILoanRepository
{
    private readonly LoanDbContext _context;

    public LoanRepository(LoanDbContext context)
    {
        _context = context;
    }

    public async Task<Loan> AddAsync(Loan loan)
    {
        await _context.Loans.AddAsync(loan);
        await _context.SaveChangesAsync();
        return loan;
    }

    public async Task UpdateAsync(Loan loan)
    {
        _context.Loans.Update(loan);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Loan>> GetAllAsync()
    {
        return await _context.Loans
            .AsNoTracking()
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<Loan?> GetByIdAsync(Guid id)
    {
        return await _context.Loans.FindAsync(id);
    }
}