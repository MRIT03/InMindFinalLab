namespace FinalLab.Domain.Events.DomainEvents;

public class AccountCreatedEvent : BaseDomainEvent
{
    public long AccountId { get; }

    public AccountCreatedEvent(bool isReverted)
    {
        IsReverting = isReverted;
    }
}