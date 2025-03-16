using System.Threading.Tasks;
using FinalLab.Domain.Entities;
using FinalLab.Domain.Events;
using FinalLab.Microservices.Logger.Contexts;

using MassTransit;
using Microsoft.Extensions.Logging;

namespace FinalLab.Microservices.Logger.Services
{
    public class LogEntryConsumer : IConsumer<LogEntryCreatedEvent>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LogEntryConsumer> _logger;

        public LogEntryConsumer(AppDbContext context, ILogger<LogEntryConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<LogEntryCreatedEvent> context)
        {
            var logEvent = context.Message;
            _logger.LogInformation("Received log entry for RequestId: {RequestId} from route: {RouteURL}",
                logEvent.Entry.RequestId, logEvent.Entry.RouteURL);

            
            logEvent.Entry.Timestamp = DateTime.SpecifyKind(logEvent.Entry.Timestamp, DateTimeKind.Unspecified);
            _context.Logs.Add(logEvent.Entry);
            await _context.SaveChangesAsync();
        }
    }
}