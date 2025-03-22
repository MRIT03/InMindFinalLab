using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinalLab.Application.Commands;
using FinalLab.Application.EventHandlers;
using FinalLab.Domain.Entities;
using FinalLab.Domain.Entities.Events.UpdateEvents;
using FinalLab.Domain.Events.DomainEvents;
using FinalLab.Infrastructure.Persistence;
using FinalLab.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;


//UpdateEvents are considered notifications in this app

namespace FinalLab.Tests
{
    public class NotificationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly IAccountRepository _accountRepository;
        private readonly IEventRepository _eventRepository;
        private readonly ITransactionRepository _transactionRepository; 

        public NotificationTests()
        {
            // Create a unique in-memory database for each test run.
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(_options);
            _accountRepository = new AccountRepository(_context);
            _eventRepository = new EventRepository(_context);
            _transactionRepository = new TransactionRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Notification Creation Tests

        [Fact]
        public async Task AccountModifiedEventHandler_CreatesNotificationAndUpdatesAccount()
        {
            // Arrange: Seed an account.
            var account = new Account { Balance = 100, Status = "Active" };
            await _accountRepository.AddAsync(account);
            await _accountRepository.SaveAsync();

            var domainEvent = new AccountModifiedEvent(account.AccountId, "Active", "Inactive", false);

            var loggerMock = new Mock<ILogger<AccountModifiedEventHandler>>();

            var handler = new AccountModifiedEventHandler(_context, loggerMock.Object, _accountRepository, _eventRepository);

            // Act: 
            await handler.Handle(domainEvent, CancellationToken.None);

            // Assert: Verifying that the account status was updated.
            var updatedAccount = (await _accountRepository.GetAllAsync()).First(a => a.AccountId == account.AccountId);
            Assert.Equal("Inactive", updatedAccount.Status);

            // Assert: Verifying that an AccountUpdateEvent notification was created.
            var notification = await _context.Events.FirstOrDefaultAsync(
                e => e is AccountUpdateEvent && ((AccountUpdateEvent)e).AccountId == account.AccountId);
            Assert.NotNull(notification);
            var accUpdateEvent = notification as AccountUpdateEvent;
            Assert.Equal("Active", accUpdateEvent.OldStatus);
            Assert.Equal("Inactive", accUpdateEvent.NewStatus);
        }

        [Fact]
        public async Task MoneyTransferredEventHandler_CreatesNotificationAndTransfersFunds()
        {
            // Arrange: Seed sender and receiver accounts.
            var sender = new Account { Balance = 200, Status = "Active" };
            var receiver = new Account { Balance = 50, Status = "Active" };
            await _accountRepository.AddAsync(sender);
            await _accountRepository.AddAsync(receiver);
            await _accountRepository.SaveAsync();

            var mediatorMock = new Mock<MediatR.IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateTransactionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Transaction
                {
                    Id = 1,
                    AccountId = sender.AccountId,
                    Amount = 75,
                    TransactionType = "Transfer",
                    Details = "Transferred money to " + receiver.AccountId,
                    Status = "success",
                    Timestamp = DateTime.UtcNow
                });

            var domainEvent = new MoneyTransferredEvent(sender.AccountId, receiver.AccountId, 75, false)
            {
                TransactionId = 1001
            };

            var loggerMock = new Mock<ILogger<MoneyTransferredEventHandler>>();

            var handler = new MoneyTransferredEventHandler(_context, loggerMock.Object, mediatorMock.Object, _eventRepository, _transactionRepository, _accountRepository);

            // Act: 
            await handler.Handle(domainEvent, CancellationToken.None);

            // Assert: Verify that the sender's balance was reduced by the transfer amount.
            var updatedSender = (await _accountRepository.GetAllAsync()).First(a => a.AccountId == sender.AccountId);
            Assert.Equal(125, updatedSender.Balance);

            // Assert: Verify that a TransactionUpdateEvent notification was created.
            var notification = await _context.Events.FirstOrDefaultAsync(e => e is TransactionUpdateEvent);
            Assert.NotNull(notification);
            var transUpdateEvent = notification as TransactionUpdateEvent;
            Assert.Equal(1001, transUpdateEvent.TransactionId);
            // For a non-reverting transfer, the mapped NewBalance is negative.
            Assert.Equal(-75, transUpdateEvent.NewBalance);
        }

        #endregion

        #region Notification Retrieval Tests

        [Fact]
        public async Task RetrieveNotifications_ByUserId_ReturnsCorrectNotifications()
        {
            // Arrange: Insert several notifications (update events) for a specific account.
            var accountId = 123;
            var notification1 = new AccountUpdateEvent
            {
                AccountId = accountId,
                OldStatus = "Active",
                NewStatus = "Inactive",
                Timestamp = DateTime.UtcNow,
                Read = false
            };
            var notification2 = new AccountUpdateEvent
            {
                AccountId = accountId,
                OldStatus = "Inactive",
                NewStatus = "Active",
                Timestamp = DateTime.UtcNow,
                Read = false
            };
            // Insert a notification for a different account.
            var notification3 = new AccountUpdateEvent
            {
                AccountId = 999,
                OldStatus = "Active",
                NewStatus = "Suspended",
                Timestamp = DateTime.UtcNow,
                Read = false
            };

            await _eventRepository.AddAsync(notification1);
            await _eventRepository.AddAsync(notification2);
            await _eventRepository.AddAsync(notification3);
            await _context.SaveChangesAsync();

            // Act: Retrieve notifications for accountId 123.
            var notifications = await _context.Events
                .OfType<AccountUpdateEvent>()
                .Where(e => e.AccountId == accountId)
                .ToListAsync();

            // Assert: Only the two notifications for account 123 should be returned.
            Assert.Equal(2, notifications.Count);
        }

        #endregion

        #region Marking Notifications as Read Tests

        [Fact]
        public async Task MarkNotificationsAsRead_UpdatesReadProperty()
        {
            // Arrange: Insert a notification.
            var notification = new AccountUpdateEvent
            {
                AccountId = 123,
                OldStatus = "Active",
                NewStatus = "Inactive",
                Timestamp = DateTime.UtcNow,
                Read = false
            };
            await _eventRepository.AddAsync(notification);
            await _context.SaveChangesAsync();

            // Act: Mark the notification as read.
            notification.Read = true;
            _context.Events.Update(notification);
            await _context.SaveChangesAsync();

            // Assert: Verify that the notification's Read property is now true.
            var updatedNotification = await _context.Events.FirstOrDefaultAsync(e => e.EventId == notification.EventId);
            Assert.NotNull(updatedNotification);
            Assert.True(updatedNotification.Read);
        }

        #endregion
    }
}
