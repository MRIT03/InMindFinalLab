using FinalLab.Domain.Events;
using FinalLab.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FinalLab.Application.Commands;
using FinalLab.Application.Services;
using FinalLab.Domain.Entities;
using FinalLab.Domain.Entities.Events.UpdateEvents;
using FinalLab.Domain.Events.DomainEvents;
using FinalLab.Infrastructure.Mappers;
using FinalLab.Persistence.Repositories;
using FinalLab.Persistence.UnitsOfWork;

namespace FinalLab.Application.EventHandlers
{
    public class MoneyTransferredEventHandler : INotificationHandler<MoneyTransferredEvent>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MoneyTransferredEventHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IEventRepository _eventRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;
        public MoneyTransferredEventHandler(ApplicationDbContext context, ILogger<MoneyTransferredEventHandler> logger, IMediator mediator, IEventRepository eventRepository, ITransactionRepository transactionRepository, IAccountRepository accountRepository)
        {
            _context = context;
            _logger = logger;
            _mediator = mediator;
            _eventRepository = eventRepository;
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
        }

        public async Task Handle(MoneyTransferredEvent notification, CancellationToken cancellationToken)
        {
            TransactionUpdateEvent loggingEvent = DomainEventMapper.Map(notification);
            var Accounts = await _accountRepository.GetAllAsync();
            var sender = Accounts.FirstOrDefault(a => a.AccountId == notification.FromAccountId);
            var receiver = Accounts.FirstOrDefault(a => a.AccountId == notification.ToAccountId);

            if (sender == null || receiver == null)
            {
                _logger.LogError("Invalid accounts for money transfer.");
                return;
            }

            if (notification.IsReverting)
            {
                IUnitOfWork transfer = new TransferFunds(
                    _accountRepository,
                    _transactionRepository,
                    toAccountId: notification.FromAccountId,  // Destination: switched
                    amount: notification.Amount,
                    fromAccountId: notification.ToAccountId     // Source: switched
                );
                await transfer.commit();
                
                var transactionReverted = new CreateTransactionCommand()
                {
                    Amount = notification.Amount,
                    AccountId = notification.ToAccountId,
                    Details = "Transferred money reverted from " + notification.FromAccountId,
                    TransactionType = "Transfer",
                    status = "Reverted"
                };
                await _mediator.Send(transactionReverted, cancellationToken);

                _logger.LogWarning("Money transfer REVERTED: From {From} to {To} - Amount: {Amount}",
                    notification.FromAccountId, notification.ToAccountId, notification.Amount);
            }
            else
            {
                if (sender.Balance < notification.Amount)
                {
                    _logger.LogError("Insufficient funds in account {FromAccountId}. Triggering rollback.", notification.FromAccountId);
                    return;
                }

                IUnitOfWork transfer = new TransferFunds(
                    _accountRepository,
                    _transactionRepository,
                    toAccountId: notification.ToAccountId,  
                    amount: notification.Amount,
                    fromAccountId: notification.FromAccountId    
                );
                await transfer.commit();

                var transactionSuccessful = new CreateTransactionCommand()
                {
                    Amount = notification.Amount,
                    AccountId = notification.FromAccountId,
                    Details = "Transferred money to " + notification.ToAccountId,
                    TransactionType = "Transfer",
                    status = "success"
                };
                await _mediator.Send(transactionSuccessful, cancellationToken);

                _logger.LogInformation("Money transferred from {From} to {To} - Amount: {Amount}",
                    notification.FromAccountId, notification.ToAccountId, notification.Amount);
            }
            await _eventRepository.AddAsync(loggingEvent);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
