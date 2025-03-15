using FinalLab.Persistence.Repositories;
using FinalLab.Domain.Entities;
namespace FinalLab.Persistence.UnitsOfWork;

public class TransferFunds : IUnitOfWork
{
    public TransferFunds()
    {
        
    }
    public TransferFunds(IAccountRepository accounts, ITransactionRepository transactionRepository, long toAccountId, decimal amount, long accountId)
    {
        _Accounts = accounts;
        _TransactionRepository = transactionRepository;
        ToAccountId = toAccountId;
        Amount = amount;
        AccountId = accountId;
        
    }

    private IAccountRepository _Accounts { get; set; }
    private ITransactionRepository _TransactionRepository { get; set; }
    
    public long ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public long AccountId { get; set; }

    
    public async Task commit()
    {
        Transfer();
        _TransactionRepository.SaveAsync();
        _Accounts.SaveAsync();
    }

    public async void Transfer()
    {
        var accounts = await _Accounts.GetAllAsync();
        var acc = accounts.Find(acc => acc.AccountId == AccountId);
        if (acc == null || acc.Balance < Amount)
        {
            throw new Exception();
        }
        var toAcc = accounts.Find(acc => acc.AccountId == ToAccountId);
        acc.Balance -= Amount;
        toAcc.Balance += Amount;
        
        Transaction transaction = new Transaction()
        {
            AccountId = AccountId,
            Amount = Amount,
            Status = "In Progress",
            Details = "Transfering funds"
        };
        await _TransactionRepository.AddAsync(transaction);
        
        
    }
}