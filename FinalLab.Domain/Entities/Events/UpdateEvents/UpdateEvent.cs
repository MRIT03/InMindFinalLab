namespace FinalLab.Domain.Entities.Events.UpdateEvents;


// These entities are meant for logging within the database 
// A mapper will be created between the respective domain event and the update event
public class UpdateEvent
{ 
    public int EventId { get; set; }
    public string OldStatus { get; set; }
    public string NewStatus { get; set; }
    public decimal OldBalance { get; set; }
    public decimal NewBalance { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsRevert { get; set; }

    // I will implement a recursive relationship between the events
    // This is for reverting events
    // Whenever I do decide to revert an event
    // I will create a new event that reverts the original event
    // I will then record the new event in the database and have it point to its parent event (The one it reverted)
    public int? ParentEventId { get; set; }
    public virtual UpdateEvent ParentEvent { get; set; }
    public virtual ICollection<UpdateEvent> RevertEvents { get; set; } = new List<UpdateEvent>();
}