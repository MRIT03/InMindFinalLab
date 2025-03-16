using FinalLab.Application.Dtos;
using FinalLab.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLab.API.Dtos;
using FinalLab.Domain.Events.DomainEvents;

namespace FinalLab.API.Controllers
{
    [Route("accounts/")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountController(IMediator mediator)
        {
            _mediator = mediator;
        }

        
        [HttpGet("common-transactions")]
        public async Task<IActionResult> GetCommonTransactions([FromQuery] List<long> accountIds)
        {
            if (accountIds == null || accountIds.Count < 2)
                return BadRequest("At least two account IDs are required.");

            var transactions = await _mediator.Send(new GetCommonTransactionsQuery(accountIds));

            return Ok(transactions);
        }

       
        [HttpGet("balance-summary/{userId}")]
        public async Task<IActionResult> GetAccountBalanceSummary(long userId)
        {
            var summaries = await _mediator.Send(new GetBalanceSummaryQuery());
            return Ok(summaries.FirstOrDefault( s => s.AccountId == userId));
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> transfer([FromBody] EventRequestDto request)
        {
            try
            {
                INotification eventToDispatch = new AccountModifiedEvent(request.AccountId, request.OldStatus,
                    request.NewStatus, request.isReverted);
                await _mediator.Publish(eventToDispatch);
                return Ok(new { Message = $"{request.EventType} event dispatched successfully." });
            }
            catch
            {
                return BadRequest(new { Message = "An error occured while processing your request." });
            }

        }
    }
}