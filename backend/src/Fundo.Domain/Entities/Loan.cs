using Fundo.Domain.Constants;

namespace Fundo.Domain.Entities;

public class Loan
{
    public Guid Id { get; set; }

    public decimal Amount { get; set; }

    public decimal CurrentBalance { get; set; }

    public string ApplicantName { get; set; }

    public string Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Validates that current balance doesn't exceed the original loan amount
    /// </summary>
    public bool IsValid()
    {
        return CurrentBalance <= Amount && CurrentBalance >= 0;
    }

    /// <summary>
    /// Updates status to "paid" if balance reaches zero
    /// </summary>
    public void UpdateStatus()
    {
        if (CurrentBalance <= 0)
        {
            Status = LoanStatus.Paid;
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}