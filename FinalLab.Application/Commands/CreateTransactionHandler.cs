using FinalLab.Application.Commands;
using FinalLab.Domain.Entities;
using FinalLab.Persistence.Repositories;
using MediatR;

namespace FinalLab.Application.Querries;

public class CreateTransactionHandler : IRequestHandler<CreateTransactionCommand, Transaction>
{
    private readonly ITransactionRepository _transactionRepository;

    public CreateTransactionHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }


    public async Task<Transaction> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        Transaction entity = new Transaction()
        {
            AccountId = request.AccountId,
            TransactionType = request.TransactionType,
            Amount = request.Amount,
            Details = request.Details,
            Status = request.status
        };
        
        await _transactionRepository.AddAsync(entity);
        return entity;
    }
}