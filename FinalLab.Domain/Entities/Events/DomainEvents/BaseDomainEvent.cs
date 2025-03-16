using MediatR;

namespace FinalLab.Domain.Events.DomainEvents;

public abstract class BaseDomainEvent : INotification
{
    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
    public bool IsReverting { get; set; } = false;
    
}