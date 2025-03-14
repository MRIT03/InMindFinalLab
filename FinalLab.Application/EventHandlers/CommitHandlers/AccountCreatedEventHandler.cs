using FinalLab.Domain.Events;
using FinalLab.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FinalLab.Domain.Entities;
using FinalLab.Domain.Events.DomainEvents;

namespace FinalLab.Application.EventHandlers
{
    public class AccountCreatedEventHandler : INotificationHandler<AccountCreatedEvent>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountCreatedEventHandler> _logger;

        public AccountCreatedEventHandler(ApplicationDbContext context, ILogger<AccountCreatedEventHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Handle(AccountCreatedEvent notification, CancellationToken cancellationToken)
        {
            AccountUpdateEvent obj = new AccountUpdateEvent();
            obj.AccountId = notification.AccountId;
            _context.UpdateEvents.AddAsync(obj, cancellationToken);
            
            if (notification.IsReverting)
            {
                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == notification.AccountId, cancellationToken);
                if (account != null)
                {
                    _context.Accounts.Remove(account);
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogWarning("Account creation REVERTED: AccountId = {AccountId}", notification.AccountId);
                }
            }
            else
            {
                _logger.LogInformation("New account created: AccountId = {AccountId}", notification.AccountId);
            }
        }
    }
}