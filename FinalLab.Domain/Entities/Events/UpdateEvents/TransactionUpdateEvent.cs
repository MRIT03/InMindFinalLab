namespace FinalLab.Domain.Entities.Events.UpdateEvents;

public class TransactionUpdateEvent : UpdateEvent
{
    public long TransactionId { get; set; }
    public string TransactionType { get; set; }
}