namespace Fundo.Application.DTO;

public class LoanResponse
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public decimal CurrentBalance { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
