namespace FinalLab.Domain.Events;

public class AccountUpdateEvent : UpdateEvent
{
    public long AccountId { get; set; }
    public AccountUpdateEvent()
    {
        timeStamp = DateTime.Now;
    }
}