using FinalLab.Domain.Events;
using FinalLab.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FinalLab.Domain.Events.DomainEvents;

namespace FinalLab.Application.EventHandlers
{
    public class MoneyTransferredEventHandler : INotificationHandler<MoneyTransferredEvent>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MoneyTransferredEventHandler> _logger;
        private readonly IMediator _mediator;

        public MoneyTransferredEventHandler(ApplicationDbContext context, ILogger<MoneyTransferredEventHandler> logger, IMediator mediator)
        {
            _context = context;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Handle(MoneyTransferredEvent notification, CancellationToken cancellationToken)
        {
            TransactionUpdateEvent obj = new TransactionUpdateEvent();
            obj.TransactionId = notification.TransactionId;
            var sender = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == notification.FromAccountId, cancellationToken);
            var receiver = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == notification.ToAccountId, cancellationToken);

            if (sender == null || receiver == null)
            {
                _logger.LogError("Invalid accounts for money transfer.");
                return;
            }

            if (notification.IsReverting)
            {
                receiver.Balance -= notification.Amount;
                sender.Balance += notification.Amount;

                _logger.LogWarning("Money transfer REVERTED: From {From} to {To} - Amount: {Amount}",
                    notification.FromAccountId, notification.ToAccountId, notification.Amount);
            }
            else
            {
                if (sender.Balance < notification.Amount)
                {
                    _logger.LogError("Insufficient funds in account {FromAccountId}. Triggering rollback.", notification.FromAccountId);
                    await _mediator.Publish(new MoneyTransferredEvent(notification.FromAccountId, notification.ToAccountId, notification.Amount, true)); // 🔥 Trigger rollback
                    return;
                }

                sender.Balance -= notification.Amount;
                receiver.Balance += notification.Amount;

                _logger.LogInformation("Money transferred from {From} to {To} - Amount: {Amount}",
                    notification.FromAccountId, notification.ToAccountId, notification.Amount);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
