using FinalLab.Common;
using FinalLab.Persistence.Repositories;
using MediatR;

namespace FinalLab.Application.Queries;

public class GetBalanceQueryHandler : IRequestHandler<GetBalanceQuery, Result<decimal>>
{
    private readonly IAccountRepository _accountRepository;

    public GetBalanceQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Result<decimal>> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        var accounts = _accountRepository.GetAllAsync().Result.ToList();
        var account = accounts.Find(a => a.AccountId == request.accountId);
        if (account == null)
        {
            return Result<decimal>.Failure("Account not found");
        }
        return Result<decimal>.Success(account.Balance);
        
    }
}