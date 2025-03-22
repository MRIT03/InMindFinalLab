using FinalLab.Common;
using FinalLab.Domain.Entities;
using FinalLab.Persistence.Repositories;
using MediatR;

namespace FinalLab.Application.Commands.CreateAccount;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Result<Account>>
{
    private readonly IAccountRepository _accountRepository;

    public CreateAccountCommandHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Result<Account>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        Account account = new Account()
        {
            Balance = request.initialBalance,
            Status = "Active"
        };
        await _accountRepository.AddAsync(account);
        await _accountRepository.SaveAsync();

        if (account.AccountId != 0) // Since EF will add the account to the db, an accountID != 0 will be generated
        {
            return Result<Account>.Success(account);
        }
        // If accountId is 0 (default value) then EF failed adding the account to the db.
        return Result<Account>.Failure("Account not created");
    }
    
}