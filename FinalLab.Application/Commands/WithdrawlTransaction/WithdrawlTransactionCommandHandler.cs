using FinalLab.Common;
using FinalLab.Domain.Entities;
using FinalLab.Persistence.Repositories;
using MediatR;

namespace FinalLab.Application.Commands.WithdrawlTransaction;

public class WithdrawlTransactionCommandHandler : IRequestHandler<WithdrawlTransactionCommand, Result<Transaction>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMediator _mediator;

    public WithdrawlTransactionCommandHandler(IMediator mediator, IAccountRepository accountRepository)
    {
        _mediator = mediator;
        _accountRepository = accountRepository;
    }

    public async Task<Result<Transaction>> Handle(WithdrawlTransactionCommand request, CancellationToken cancellationToken)
    {
        if (request.amount <= 0)
        {
            return Result<Transaction>.Failure("Withdrawl transaction amount must be greater than 0");
        }
        var accounts = _accountRepository.GetAllAsync().Result.ToList();
        var account = accounts.FirstOrDefault(a => a.AccountId == request.accountId);

        if (account == null)
        {
            return Result<Transaction>.Failure("Account not found");
        }

        if (account.Balance < request.amount)
        {
            return Result<Transaction>.Failure("Insufficient funds");
        }
        account.Balance -= request.amount;
        CreateTransactionCommand command = new CreateTransactionCommand()
        {
            AccountId = account.AccountId,
            Amount = request.amount,
            TransactionType = "Withdrawl",
            Details = "Withdrawl transaction",
            status = "Pending"
        };
        var transaction = await _mediator.Send(command, cancellationToken);
        await _accountRepository.SaveAsync();
        return Result<Transaction>.Success(transaction);
        
    }
}