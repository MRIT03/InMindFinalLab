namespace FinalLab.Domain.Events.DomainEvents;

public class AccountCreatedEvent : BaseDomainEvent
{
    public long AccountId { get; }

    public AccountCreatedEvent(long accountId, bool isReverted)
    {
        AccountId = accountId;
        IsReverting = isReverted;
    }
}