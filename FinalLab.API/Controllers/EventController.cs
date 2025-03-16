using FinalLab.Domain.Events;
using FinalLab.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLab.API.Dtos;
using FinalLab.Domain.Entities.Events.UpdateEvents;
using FinalLab.Domain.Events.DomainEvents;

namespace FinalLab.API.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IMediator mediator, ApplicationDbContext context, ILogger<EventsController> logger)
        {
            _mediator = mediator;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Dispatches a domain event for a transaction or account action.
        /// Example: POST /api/events
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DispatchEvent([FromBody] EventRequestDto request)
        {
            if (string.IsNullOrEmpty(request.EventType))
                return BadRequest("EventType is required.");

            INotification eventToDispatch;

            switch (request.EventType.ToLower())
            {
                case "moneytransferred":
                    eventToDispatch = new MoneyTransferredEvent(request.FromAccountId, request.ToAccountId, request.Amount, request.isReverted);
                    break;

                case "accountcreated":
                    eventToDispatch = new AccountCreatedEvent(request.isReverted);
                    break;

                case "accountmodified":
                    eventToDispatch = new AccountModifiedEvent(request.AccountId, request.OldStatus, request.NewStatus, request.isReverted);
                    break;

                default:
                    return BadRequest("Invalid EventType. Supported: MoneyTransferred, AccountCreated, AccountModified.");
            }

            await _mediator.Publish(eventToDispatch);
            _logger.LogInformation("Dispatched {EventType} event.", request.EventType);

            return Ok(new { Message = $"{request.EventType} event dispatched successfully." });
        }

        /// <summary>
        /// Retrieves all events associated with a specific transaction.
        /// Example: GET /api/events/{transactionId}
        /// </summary>
        [HttpGet("{transactionId}")]
        public async Task<IActionResult> GetEventsByTransaction(long transactionId)
        {
            var events = await _context.Events
                .Where(e => (e is TransactionUpdateEvent) && ((TransactionUpdateEvent)e).TransactionId == transactionId)
                .ToListAsync();

            if (events.Count == 0)
                return NotFound($"No events found for TransactionId {transactionId}.");

            return Ok(events);
        }
    }
}
