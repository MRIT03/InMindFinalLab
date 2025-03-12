using MediatR;
using FinalLab.Domain.Entities;
using System.Collections.Generic;

namespace FinalLab.Application.Queries
{
    public class GetCommonTransactionsQuery : IRequest<List<Transaction>>
    {
        public List<long> AccountIds { get; }

        public GetCommonTransactionsQuery(List<long> accountIds)
        {
            AccountIds = accountIds;
        }
    }
}