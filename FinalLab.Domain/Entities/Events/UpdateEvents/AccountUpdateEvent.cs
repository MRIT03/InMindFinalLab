namespace FinalLab.Domain.Entities.Events.UpdateEvents;

public class AccountUpdateEvent : UpdateEvent
{
    public long AccountId { get; set; }
    
}