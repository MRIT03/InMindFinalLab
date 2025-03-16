using FinalLab.Persistence.Repositories;

namespace FinalLab.Persistence.UnitsOfWork;

public class ChangeAccountStatus : IUnitOfWork
{
    public ChangeAccountStatus(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    

    public string NewStatus { get; set; }
    public long AccountId { get; set; }
    private readonly IAccountRepository _accountRepository;


    public async Task commit()
    {
        await setStatusAsync();
        _accountRepository.SaveAsync();
    }

    private async Task setStatusAsync()
    {
        var accounts = await _accountRepository.GetAllAsync();
        accounts.First(a => a.AccountId == AccountId).Status = NewStatus;
    }
}