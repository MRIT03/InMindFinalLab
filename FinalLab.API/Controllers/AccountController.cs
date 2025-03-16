using FinalLab.Application.Dtos;
using FinalLab.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLab.API.Dtos;
using FinalLab.Domain.Events.DomainEvents;
using FinalLab.Persistence.Repositories;
using Microsoft.Extensions.Localization;

namespace FinalLab.API.Controllers
{
    [Route("accounts/")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;
        private readonly IAccountRepository _accountRepository;

        public AccountController(IMediator mediator, IAccountRepository accountRepository, IStringLocalizer<SharedResource> sharedLocalizer)
        {
            _mediator = mediator;
            _accountRepository = accountRepository;
            _sharedLocalizer = sharedLocalizer;
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

        [HttpGet("{AccountId}/details")]
        public async Task<IActionResult> GetAccountDetails(long accountId)
        {
            var accounts = await _accountRepository.GetAllAsync();
            var account = accounts.FirstOrDefault(a=>a.AccountId == accountId);
            if (account != null)
            {
                string message = _sharedLocalizer["Id"] + ": " + accountId + "\n" + _sharedLocalizer["Balance"] + ": " + account.Balance;
                return Ok(message);
            }
            return BadRequest(new { message = _sharedLocalizer["error"] });
        }
    }
}