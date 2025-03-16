// File: Microservices/Logger/Middleware/LoggingMiddleware.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLab.Domain.Entities;
using FinalLab.Domain.Events;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinalLab.API.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, IPublishEndpoint publishEndpoint, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Build the log entry from request information
            var logEntry = new LogEntry
            {
                RequestId = Guid.NewGuid(),
                RouteURL = context.Request.Path,
                Timestamp = DateTime.UtcNow,
                RequestData = new Dictionary<string, object>
                {
                    { "Method", context.Request.Method },
                    { "Query", context.Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString()) },
                    { "Headers", context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()) }
                }
            };

            // Publish the log event
            var logEvent = new LogEntryCreatedEvent { Entry = logEntry };
            await _publishEndpoint.Publish(logEvent);
            _logger.LogInformation("Published log entry event for request: {RequestId}", logEntry.RequestId);

            // Continue processing the request pipeline
            await _next(context);
        }
    }
}