using FinalLab.Application.Queries;
using FinalLab.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinalLab.Domain.Entities;

namespace FinalLab.Application.Handlers
{
    public class GetCommonTransactionsHandler : IRequestHandler<GetCommonTransactionsQuery, List<Transaction>>
    {
        private readonly ApplicationDbContext _context;

        public GetCommonTransactionsHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Transaction>> Handle(GetCommonTransactionsQuery request, CancellationToken cancellationToken)
        {
            var transactions = await _context.Transactions
                .Where(t => request.AccountIds.Contains(t.AccountId))
                .ToListAsync(cancellationToken);

            // Find common transactions (same TransactionType or Amount)
            var commonTransactions = transactions
                .GroupBy(t => new { t.TransactionType, t.Amount }) 
                .Where(g => g.Count() > 1) 
                .SelectMany(g => g) 
                .Distinct()
                .ToList();

            return commonTransactions;
        }
    }
}