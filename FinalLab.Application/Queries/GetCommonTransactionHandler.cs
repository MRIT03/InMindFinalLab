using FinalLab.Application.Queries;
using FinalLab.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinalLab.Domain.Entities;
using FinalLab.Persistence.Repositories;

namespace FinalLab.Application.Handlers
{
    public class GetCommonTransactionsHandler : IRequestHandler<GetCommonTransactionsQuery, List<List<Transaction>>>
    {
        
        private readonly TransactionRepository _transactionRepository;

        public GetCommonTransactionsHandler(TransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
            
        }

        // Okay here is how we're going to do this!
        // We will first get all the transactions, and then we select the ones related to the wanted AccountIds
        // We will group transactions twice, once based on amount, and once based on type
        // We will then iterate over every transaction in the grp
        // The ones that match on type or amount, we will add them to a list
        // The output is a list of lists of transactions where the sub lists contains transactions that match in type/amount
        
        // Now this is the genius part. To make sure that transactions match on all users
        // for every group in the transactions grouped by type/amount
        // we will see if the count of DISTINCT Account IDS matches the SIZE of our input array
        // If it does then every user in the input array has at least one transaction of that type/amount
        // We will use this to determine whether we add the grp to the output list or not
        public async Task<List<List<Transaction>>> Handle(GetCommonTransactionsQuery request, CancellationToken cancellationToken)
        {
            var accountIds = request.AccountIds;
            
            
            
            var transactions = await _transactionRepository.GetAllAsync();
            transactions =  transactions    
                .Where(t => request.AccountIds.Contains(t.AccountId))
                .ToList();

            var commonTransactions = transactions
                .Where(t => accountIds.Contains(t.AccountId)) 
                .ToList(); 

            var groupedByAmount = commonTransactions
                .GroupBy(t => t.Amount)
                .Where(g => g.Select(t => t.AccountId).Distinct().Count() == accountIds.Count()) 
                .Select(g => g.ToList())
                .ToList();

            var groupedByType = commonTransactions
                .GroupBy(t => t.TransactionType)
                .Where(g => g.Select(t => t.AccountId).Distinct().Count() == accountIds.Count()) 
                .Select(g => g.ToList())
                .ToList();

            
            var result = groupedByAmount.Concat(groupedByType).ToList();
            return result;

        }
    }
}