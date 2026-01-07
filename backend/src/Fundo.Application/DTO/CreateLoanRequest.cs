using System.ComponentModel.DataAnnotations;

namespace Fundo.Application.DTO;

public class CreateLoanRequest
{
    [Required]
    [Range(0.01, 1000000.00, ErrorMessage = "Loan amount must be between $0.01 and $1,000,000")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Applicant name must be between 2 and 100 characters")]
    public string ApplicantName { get; set; } = string.Empty;
}
