using FinalLab.Common;
using FinalLab.Domain.Entities;
using FinalLab.Persistence.Repositories;
using MediatR;

namespace FinalLab.Application.Commands.DepositTransaction;

public class DepositTransactionCommandHandler : IRequestHandler<DepositTransactionCommand, Result<Transaction>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMediator _mediator;

    public DepositTransactionCommandHandler(IAccountRepository accountRepository, IMediator mediator)
    {
        _accountRepository = accountRepository;
        _mediator = mediator;
    }

    public async Task<Result<Transaction>> Handle(DepositTransactionCommand request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return Result<Transaction>.Failure("Deposit transaction amount must be greater than 0");
        }
        var accounts = _accountRepository.GetAllAsync().Result.ToList();
        var account = accounts.FirstOrDefault(a => a.AccountId == request.AccountId);
        if (account == null)
        {
            return Result<Transaction>.Failure("Account not found");
        }
        account.Balance += request.Amount;
        CreateTransactionCommand command = new CreateTransactionCommand()
        {
            AccountId = account.AccountId,
            Amount = request.Amount,
            Details = "Deposit transaction",
            status = "In progress",
            TransactionType = "Deposit"
        };
        var transaction = await _mediator.Send(command);
        await _accountRepository.SaveAsync();
        return Result<Transaction>.Success(transaction);
    }
}