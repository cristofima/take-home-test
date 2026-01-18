using Fundo.Domain.Constants;

namespace Fundo.Domain.Entities;

public class Loan
{
    public Guid Id { get; private set; }
    public decimal Amount { get; private set; }
    public decimal CurrentBalance { get; private set; }
    public string ApplicantName { get; private set; }
    public string Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Internal constructor for EF Core, seed data, and tests.
    /// Allows full control over all properties including Id and timestamps.
    /// Accessible via InternalsVisibleTo in Infrastructure and Tests.
    /// </summary>
    internal Loan(
        Guid id,
        decimal amount,
        decimal currentBalance,
        string applicantName,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        Amount = amount;
        CurrentBalance = currentBalance;
        ApplicantName = applicantName;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private Loan() { }

    /// <summary>
    /// Creates a new loan with proper validation and business rules.
    /// </summary>
    /// <param name="amount">The loan amount (must be positive)</param>
    /// <param name="applicantName">The name of the loan applicant (required)</param>
    /// <returns>A new valid Loan entity</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static Loan Create(decimal amount, string applicantName)
    {
        // Validation
        if (amount <= 0)
            throw new ArgumentException("Loan amount must be greater than zero.", nameof(amount));

        if (string.IsNullOrWhiteSpace(applicantName))
            throw new ArgumentException("Applicant name is required.", nameof(applicantName));

        // Create loan with business rules
        var now = DateTimeOffset.UtcNow;
        return new Loan
        {
            Id = Guid.NewGuid(),
            Amount = amount,
            CurrentBalance = amount, // Starts with full amount
            ApplicantName = applicantName.Trim(),
            Status = LoanStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Applies a payment to the loan, reducing the current balance.
    /// Automatically updates status to "paid" when balance reaches zero.
    /// </summary>
    /// <param name="amount">The payment amount (must be positive and not exceed balance)</param>
    /// <exception cref="ArgumentException">Thrown when amount is invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when payment exceeds balance</exception>
    public void ApplyPayment(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

        if (amount > CurrentBalance)
            throw new InvalidOperationException(
                $"Payment amount ({amount:C}) cannot exceed current balance ({CurrentBalance:C}).");

        CurrentBalance -= amount;
        UpdateStatus();

        if (!IsValid())
            throw new InvalidOperationException("Loan entered an invalid state after payment.");
    }

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
    private void UpdateStatus()
    {
        if (CurrentBalance <= 0)
        {
            Status = LoanStatus.Paid;
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}