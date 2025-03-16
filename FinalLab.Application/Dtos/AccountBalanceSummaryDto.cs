namespace FinalLab.Application.Dtos;

public class AccountBalanceSummaryDto
{
    public long AccountId { get; set; }
    public decimal TotalDeposits { get; set; }
    public decimal TotalWithdrawals { get; set; }
    public decimal CurrentBalance { get; set; }
}
