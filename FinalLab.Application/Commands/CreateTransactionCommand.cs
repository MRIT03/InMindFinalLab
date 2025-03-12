using System.Transactions;
using MediatR;
using Transaction = FinalLab.Domain.Entities.Transaction;

namespace FinalLab.Application.Commands;

public class CreateTransactionCommand : IRequest, IRequest<Transaction>
{
    public long AccountId { get; set; }
    public string TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string status { get; set; }
    public string Details { get; set; }
}