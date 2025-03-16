using FinalLab.Domain.Events;
using FinalLab.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FinalLab.Domain.Entities.Events.UpdateEvents;
using FinalLab.Domain.Events.DomainEvents;
using FinalLab.Infrastructure.Mappers;
using FinalLab.Persistence.Repositories;
using FinalLab.Persistence.UnitsOfWork;


// THis file needs to be updated with repos


namespace FinalLab.Application.EventHandlers
{
    public class AccountModifiedEventHandler : INotificationHandler<AccountModifiedEvent>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountModifiedEventHandler> _logger;
        private readonly IEventRepository _eventRepository;
        private readonly IAccountRepository _accountRepository;

        public AccountModifiedEventHandler(ApplicationDbContext context, ILogger<AccountModifiedEventHandler> logger, IAccountRepository accountRepository, IEventRepository eventRepository)
        {
            _context = context;
            _logger = logger;
            _accountRepository = accountRepository;
            _eventRepository = eventRepository;
        }

        public async Task Handle(AccountModifiedEvent notification, CancellationToken cancellationToken)
        {
            AccountUpdateEvent LoggingEvent = DomainEventMapper.Map(notification);

            var uow = new ChangeAccountStatus(_accountRepository)
            {
                NewStatus = notification.NewStatus,
                AccountId = notification.AccountId,
            };
            var accounts = await _accountRepository.GetAllAsync();
            var account = accounts.FirstOrDefault(a => a.AccountId == notification.AccountId);
            if (account == null)
            {
                _logger.LogError("Account not found for modification: FromAccountId = {FromAccountId}", notification.AccountId);
                return;
            }

            if (notification.IsReverting)
            {
                uow.NewStatus = notification.OldStatus;
                await uow.commit();
                _logger.LogWarning("Account modification REVERTED: FromAccountId = {FromAccountId}, Restored Status = {OldStatus}",
                    notification.AccountId, notification.OldStatus);
            }
            else
            {
                await uow.commit();
                _logger.LogInformation("Account modified: FromAccountId = {FromAccountId}, New Status = {NewStatus}",
                    notification.AccountId, notification.NewStatus);
            }
            await _eventRepository.AddAsync(LoggingEvent);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}