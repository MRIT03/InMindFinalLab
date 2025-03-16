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
                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == notification.AccountId, cancellationToken);
                if (account != null)
                {
                    _context.Accounts.Remove(account);
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogWarning("Account creation REVERTED: AccountId = {AccountId}", notification.AccountId);
                }
                else
                {
                    _logger.LogWarning("Account deletion FAILED: AccountId = {AccountId}. No user with such Id", notification.AccountId);
                }
            }
            else
            {
                Account acc = new Account();
                await _accountRepository.AddAsync(acc);
                _logger.LogInformation("New account created: AccountId = {AccountId}", acc.AccountId);
            }
            await _eventRepository.AddAsync(LoggingEvent);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}