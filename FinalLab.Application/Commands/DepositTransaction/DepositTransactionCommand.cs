using FinalLab.Common;
using FinalLab.Domain.Entities;
using MediatR;

namespace FinalLab.Application.Commands.DepositTransaction;

public class DepositTransactionCommand : IRequest<Result<Transaction>>
{
    public long AccountId { get; set; }
    public decimal Amount { get; set; }
    
}