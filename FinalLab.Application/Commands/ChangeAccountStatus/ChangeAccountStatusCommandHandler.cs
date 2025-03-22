using FinalLab.Common;
using FinalLab.Domain.Entities;
using FinalLab.Persistence.Repositories;
using FinalLab.Persistence.UnitsOfWork;
using MediatR;

namespace FinalLab.Application.Commands.ChangeAccountStatus;

public class ChangeAccountStatusCommandHandler : IRequestHandler<ChangeAccountStatusCommand, Result<Account>>
{
    private readonly IAccountRepository _accountRepository;

    public ChangeAccountStatusCommandHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Result<Account>> Handle(ChangeAccountStatusCommand request, CancellationToken cancellationToken)
    {
        var accounts = _accountRepository.GetAllAsync().Result.ToList();
        var account = accounts.FirstOrDefault(a => a.AccountId == request.accountId);
        if (account == null)
        {
            return Result<Account>.Failure("Account not found");
        }

        IUnitOfWork changeStatus = new Persistence.UnitsOfWork.ChangeAccountStatus(_accountRepository)
        {
            AccountId = account.AccountId,
            NewStatus = request.newStatus
        };
        await changeStatus.commit();
        return Result<Account>.Success(account);
    }
}