using MediatR;

namespace FinalLab.Domain.Entities;

public class LogEntryCreatedEvent : INotification
{
    public LogEntry Entry { get; set; }
}