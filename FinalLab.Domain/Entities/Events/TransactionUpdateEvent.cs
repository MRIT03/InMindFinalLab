namespace FinalLab.Domain.Events;

public class TransactionUpdateEvent : UpdateEvent
{
    public long TransactionId { get; set; }
    public TransactionUpdateEvent()
    {
        timeStamp = DateTime.Now;
    }
}