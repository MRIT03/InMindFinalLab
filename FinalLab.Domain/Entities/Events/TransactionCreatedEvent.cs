using MediatR;
using System;

namespace FinalLab.Domain.Events
{
    public class TransactionCreatedEvent : INotification
    {
        public long TransactionId { get; }
        public long AccountId { get; }
        public decimal Amount { get; }
        public string TransactionType { get; }

        public TransactionCreatedEvent(long transactionId, long accountId, decimal amount, string transactionType)
        {
            TransactionId = transactionId;
            AccountId = accountId;
            Amount = amount;
            TransactionType = transactionType;
        }
    }
}