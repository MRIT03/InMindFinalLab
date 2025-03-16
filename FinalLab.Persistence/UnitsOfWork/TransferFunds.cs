using FinalLab.Persistence.Repositories;
using FinalLab.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace FinalLab.Persistence.UnitsOfWork
{
    public class TransferFunds : IUnitOfWork
    {
        private readonly IAccountRepository _accounts;
        private readonly ITransactionRepository _transactionRepository;

        public long ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public long FromAccountId { get; set; }

        public TransferFunds(
            IAccountRepository accounts, 
            ITransactionRepository transactionRepository, 
            long toAccountId, 
            decimal amount, 
            long fromAccountId)
        {
            _accounts = accounts ?? throw new ArgumentNullException(nameof(accounts));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            ToAccountId = toAccountId;
            Amount = amount;
            FromAccountId = fromAccountId;
        }
        
        

        public async Task commit()
        {
            await TransferAsync();
            await _transactionRepository.SaveAsync();
        }

        public async Task TransferAsync()
        {
            var accounts = await _accounts.GetAllAsync();

            // Find source account and check sufficient funds.
            var fromAccount = accounts.Find(a => a.AccountId == FromAccountId);
            if (fromAccount == null || fromAccount.Balance < Amount)
            {
                throw new Exception("Source account not found or insufficient funds.");
            }

            // Find destination account and ensure it exists.
            var toAccount = accounts.Find(a => a.AccountId == ToAccountId);
            if (toAccount == null)
            {
                throw new Exception("Destination account not found.");
            }

            // Update balances.
            fromAccount.Balance -= Amount;
            toAccount.Balance += Amount;

            // Create transaction record.
            Transaction transaction = new Transaction
            {
                AccountId = FromAccountId,
                Amount = Amount,
                Status = "In Progress",
                Details = "Transferring funds",
                TransactionType = "Transfer"
            };

            await _transactionRepository.AddAsync(transaction);
        }
    }
}
