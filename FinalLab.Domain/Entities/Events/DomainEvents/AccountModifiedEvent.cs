namespace FinalLab.Domain.Events.DomainEvents;

public class AccountModifiedEvent : BaseDomainEvent
{
    public long AccountId { get; }
    public string OldStatus { get; }
    public string NewStatus { get; }

    public AccountModifiedEvent(long accountId, string oldStatus, string newStatus, bool revert)
    {
        AccountId = accountId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        IsReverting = revert;
    }
}