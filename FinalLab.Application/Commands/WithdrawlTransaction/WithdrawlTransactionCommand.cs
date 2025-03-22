using FinalLab.Common;
using FinalLab.Domain.Entities;
using MediatR;

namespace FinalLab.Application.Commands.WithdrawlTransaction;

public class WithdrawlTransactionCommand : IRequest<Result<Transaction>>
{
    public long accountId;
    public decimal amount;
}