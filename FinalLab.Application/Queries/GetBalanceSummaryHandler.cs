using FinalLab.Application.Dtos;
using FinalLab.Application.Queries;
using FinalLab.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinalLab.Persistence.Repositories;

namespace FinalLab.Application.Handlers
{
    public class GetAccountBalanceSummaryHandler : IRequestHandler<GetBalanceSummaryQuery, List<AccountBalanceSummaryDto>>
    {
        private readonly IAccountRepository _repository;
        private readonly ITransactionRepository _transactionRepository;

        public GetAccountBalanceSummaryHandler(IAccountRepository repository, ITransactionRepository transactionRepository)
        {
            _repository = repository;
            _transactionRepository = transactionRepository;
        }

        public async Task<List<AccountBalanceSummaryDto>> Handle(GetBalanceSummaryQuery request, CancellationToken cancellationToken)
        {
            var accounts = await _repository.GetAllAsync();
            var transactions = await _transactionRepository.GetAllAsync();
            var summary = accounts
                .Select(account => new AccountBalanceSummaryDto
                {
                    AccountId = account.AccountId,
                    TotalDeposits = transactions
                        .Where(t => t.AccountId == account.AccountId && t.TransactionType == "Deposit")
                        .Sum(t => (decimal?)t.Amount) ?? 0,

                    TotalWithdrawals = transactions
                        .Where(t => t.AccountId == account.AccountId && t.TransactionType == "Withdrawal")
                        .Sum(t => (decimal?)t.Amount) ?? 0,

                    CurrentBalance = account.Balance
                }).ToList();

            return summary;
        }
    }
}