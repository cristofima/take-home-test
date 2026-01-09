using Fundo.Application.DTO;
using Fundo.Domain.Entities;
namespace Fundo.Application.Utils;

public static class LoanUtils
{
    public static LoanResponse MapToResponse(Loan loan)
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