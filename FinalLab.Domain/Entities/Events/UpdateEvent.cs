namespace FinalLab.Domain.Events;

public class UpdateEvent
{ 
    public int EventId { get; set; }
    public string oldStatus {get; set;}
    public string newStatus {get; set;}
    public decimal oldBalance {get; set;}
    public decimal newBalance {get; set;}
    public DateTime timeStamp {get; set;}
}