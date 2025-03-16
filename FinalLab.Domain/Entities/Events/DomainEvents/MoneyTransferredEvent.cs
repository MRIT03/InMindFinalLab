namespace FinalLab.Domain.Events.DomainEvents;

public class MoneyTransferredEvent : BaseDomainEvent
{
    public long TransactionId { get; set; }
    public long FromAccountId { get; }
    public long ToAccountId { get; }
    public decimal Amount { get; }

    public MoneyTransferredEvent(long fromAccountId, long toAccountId, decimal amount, bool reverted)
    {
        FromAccountId = fromAccountId;
        ToAccountId = toAccountId;
        Amount = amount;
        IsReverting = reverted;
    }
}