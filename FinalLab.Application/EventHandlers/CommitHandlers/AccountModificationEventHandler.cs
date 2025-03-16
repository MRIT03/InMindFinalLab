using FinalLab.Domain.Events;
using FinalLab.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FinalLab.Domain.Entities.Events.UpdateEvents;
using FinalLab.Domain.Events.DomainEvents;

namespace FinalLab.Application.EventHandlers
{
    public class AccountModifiedEventHandler : INotificationHandler<AccountModifiedEvent>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountModifiedEventHandler> _logger;

        public AccountModifiedEventHandler(ApplicationDbContext context, ILogger<AccountModifiedEventHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Handle(AccountModifiedEvent notification, CancellationToken cancellationToken)
        {
            AccountUpdateEvent obj = new AccountUpdateEvent();
            obj.AccountId = notification.AccountId;
            _context.UpdateEvents.AddAsync(obj, cancellationToken);
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == notification.AccountId, cancellationToken);

            if (account == null)
            {
                _logger.LogError("Account not found for modification: AccountId = {AccountId}", notification.AccountId);
                return;
            }

            if (notification.IsReverting)
            {
                account.Status = notification.OldStatus;
                _logger.LogWarning("Account modification REVERTED: AccountId = {AccountId}, Restored Status = {OldStatus}",
                    notification.AccountId, notification.OldStatus);
            }
            else
            {
                account.Status = notification.NewStatus;
                _logger.LogInformation("Account modified: AccountId = {AccountId}, New Status = {NewStatus}",
                    notification.AccountId, notification.NewStatus);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}