using FinalLab.Domain.Events;
using FinalLab.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FinalLab.Domain.Entities;
using FinalLab.Domain.Entities.Events.UpdateEvents;
using FinalLab.Domain.Events.DomainEvents;
using FinalLab.Infrastructure.Mappers;
using FinalLab.Persistence.Repositories;


// THis file needs to be updated with repos

namespace FinalLab.Application.EventHandlers
{
    public class AccountCreatedEventHandler : INotificationHandler<AccountCreatedEvent>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountCreatedEventHandler> _logger;
        private readonly IEventRepository _eventRepository;
        private readonly IAccountRepository _accountRepository;

        public AccountCreatedEventHandler(ApplicationDbContext context, ILogger<AccountCreatedEventHandler> logger, IEventRepository eventRepository, IAccountRepository accountRepository)
        {
            _context = context;
            _logger = logger;
            _eventRepository = eventRepository;
            _accountRepository = accountRepository;
        }

        public async Task Handle(AccountCreatedEvent notification, CancellationToken cancellationToken)
        {
            AccountUpdateEvent LoggingEvent = DomainEventMapper.Map(notification);
            
            
            if (notification.IsReverting)
            {
                var Accounts = await _accountRepository.GetAllAsync();
                var account = Accounts.FirstOrDefault(a => a.AccountId == notification.AccountId);
                if (account != null)
                {
                    _accountRepository.DeleteAsync(account);
                    await _accountRepository.SaveAsync();
                    _logger.LogWarning("Account creation REVERTED: FromAccountId = {FromAccountId}", notification.AccountId);
                }
                else
                {
                    _logger.LogWarning("Account deletion FAILED: FromAccountId = {FromAccountId}. No user with such Id", notification.AccountId);
                }
            }
            else
            {
                Account acc = new Account();
                await _accountRepository.AddAsync(acc);
                _logger.LogInformation("New account created: FromAccountId = {FromAccountId}", acc.AccountId);
            }
            await _eventRepository.AddAsync(LoggingEvent);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}