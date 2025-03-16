using FinalLab.Domain.Entities.Events.UpdateEvents;
using FinalLab.Domain.Events;
using FinalLab.Domain.Events.DomainEvents;

namespace FinalLab.Infrastructure.Mappers
{
    public static class DomainEventMapper
    {
        /// <summary>
        /// Maps a MoneyTransferredEvent to a TransactionUpdateEvent.
        /// </summary>
        public static TransactionUpdateEvent Map(MoneyTransferredEvent domainEvent)
        {
            var output = new TransactionUpdateEvent
            {
                TransactionId = domainEvent.TransactionId,
                NewBalance = - domainEvent.Amount, 
                Timestamp = domainEvent.OccurredOn,
                IsRevert = domainEvent.IsReverting
            };
            // If we are reverting we will gain the transferred amount, if not we lose it.
            if (domainEvent.IsReverting)
            {
                output.ParentEventId = domainEvent.ParentEventId;
                output.NewBalance = - output.NewBalance;
            }
            
            return output;
            
        }

        /// <summary>
        /// Maps an AccountCreatedEvent to an AccountUpdateEvent.
        /// </summary>
        public static AccountUpdateEvent Map(AccountCreatedEvent domainEvent)
        {
            
            var output =  new AccountUpdateEvent
            {
                AccountId = domainEvent.AccountId,
                OldStatus = "Not Created",
                NewStatus = "Created",
                Timestamp = domainEvent.OccurredOn,
                IsRevert = domainEvent.IsReverting
            };
            if (domainEvent.IsReverting)
            {
                output.ParentEventId = domainEvent.ParentEventId;
                output.OldStatus = "Created";
                output.NewStatus = "Not Created";
            }
            return output;
        }

        /// <summary>
        /// Maps an AccountModifiedEvent to an AccountUpdateEvent.
        /// </summary>
        public static AccountUpdateEvent Map(AccountModifiedEvent domainEvent)
        {
            var output  = new AccountUpdateEvent
            {
                AccountId = domainEvent.AccountId,
                OldStatus = domainEvent.OldStatus,
                NewStatus = domainEvent.NewStatus,
                Timestamp = domainEvent.OccurredOn,
                IsRevert = domainEvent.IsReverting
            };
            if (domainEvent.IsReverting)
            {
                output.ParentEventId = domainEvent.ParentEventId;
                output.OldStatus = output.NewStatus;
                output.NewStatus = domainEvent.OldStatus;
            }
            return output;
        }
    }
}
