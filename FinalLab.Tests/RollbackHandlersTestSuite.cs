using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinalLab.Application.Commands.RollbackCommands;
using FinalLab.Common;
using FinalLab.Domain.Entities.Events.UpdateEvents;
using FinalLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinalLab.Tests
{
    public class RollbackTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly DbContextOptions<ApplicationDbContext> _options;
        
        public RollbackTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(_options);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task RollbackEventsByDayCommandHandler_RevertsEventsForSpecificDay()
        {
            // Arrange: Create two events on a specific day and one on another day.
            var targetDay = new DateTime(2025, 3, 21);
            var event1 = new AccountUpdateEvent 
            { 
                AccountId = 1, 
                OldStatus = "Active", 
                NewStatus = "Inactive", 
                Timestamp = targetDay.AddHours(10), 
                Read = false, 
                IsRevert = false 
            };
            var event2 = new TransactionUpdateEvent 
            { 
                TransactionId = 101, 
                TransactionType = "Deposit", 
                Timestamp = targetDay.AddHours(12), 
                Read = false, 
                IsRevert = false 
            };
            var event3 = new AccountUpdateEvent 
            { 
                AccountId = 2, 
                OldStatus = "Active", 
                NewStatus = "Suspended", 
                Timestamp = targetDay.AddDays(1), 
                Read = false, 
                IsRevert = false 
            };

            await _context.Events.AddRangeAsync(event1, event2, event3);
            await _context.SaveChangesAsync();

            var handler = new RollbackEventsByDayCommandHandler(_context);
            var command = new RollbackEventsByDayCommand { Day = targetDay };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert: event1 and event2 should be marked as reverted; event3 remains unchanged.
            Assert.True(result.IsSuccess);
            var events = await _context.Events.ToListAsync();
            Assert.True(events.First(e => e.EventId == event1.EventId).IsRevert);
            Assert.True(events.First(e => e.EventId == event2.EventId).IsRevert);
            Assert.False(events.First(e => e.EventId == event3.EventId).IsRevert);
        }

        [Fact]
        public async Task RollbackEventsByAccountCommandHandler_RevertsEventsForSpecificAccount()
        {
            // Arrange: Create events for account 5 and account 6.
            var event1 = new AccountUpdateEvent 
            { 
                AccountId = 5, 
                OldStatus = "Active", 
                NewStatus = "Inactive", 
                Timestamp = DateTime.UtcNow, 
                Read = false, 
                IsRevert = false 
            };
            var event2 = new AccountUpdateEvent 
            { 
                AccountId = 5, 
                OldStatus = "Inactive", 
                NewStatus = "Active", 
                Timestamp = DateTime.UtcNow, 
                Read = false, 
                IsRevert = false 
            };
            var event3 = new AccountUpdateEvent 
            { 
                AccountId = 6, 
                OldStatus = "Active", 
                NewStatus = "Suspended", 
                Timestamp = DateTime.UtcNow, 
                Read = false, 
                IsRevert = false 
            };

            await _context.Events.AddRangeAsync(event1, event2, event3);
            await _context.SaveChangesAsync();

            var handler = new RollbackEventsByAccountCommandHandler(_context);
            var command = new RollbackEventsByAccountCommand { AccountId = 5 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert: Only events for account 5 should be marked as reverted.
            Assert.True(result.IsSuccess);
            var eventsForAccount5 = await _context.Events
                .OfType<AccountUpdateEvent>()
                .Where(e => e.AccountId == 5)
                .ToListAsync();
            Assert.All(eventsForAccount5, e => Assert.True(e.IsRevert));

            // Ensure event for account 6 is unchanged.
            var eventForAccount6 = await _context.Events
                .OfType<AccountUpdateEvent>()
                .FirstOrDefaultAsync(e => e.AccountId == 6);
            Assert.False(eventForAccount6.IsRevert);
        }

        [Fact]
        public async Task FilterEvents_ByAccountAndTransactionType_ReturnsCorrectResults()
        {
            // Arrange: Create several TransactionUpdateEvents with different TransactionType values.
            var event1 = new TransactionUpdateEvent 
            { 
                TransactionId = 201, 
                TransactionType = "Deposit", 
                Timestamp = DateTime.UtcNow, 
                Read = false, 
                IsRevert = false 
            };
            var event2 = new TransactionUpdateEvent 
            { 
                TransactionId = 202, 
                TransactionType = "Withdrawl", 
                Timestamp = DateTime.UtcNow, 
                Read = false, 
                IsRevert = false 
            };
            var event3 = new TransactionUpdateEvent 
            { 
                TransactionId = 203, 
                TransactionType = "Transfer", 
                Timestamp = DateTime.UtcNow, 
                Read = false, 
                IsRevert = false 
            };

            await _context.Events.AddRangeAsync(event1, event2, event3);
            await _context.SaveChangesAsync();

            // Act: Filter events with TransactionType "Deposit".
            var depositEvents = await _context.Events
                .OfType<TransactionUpdateEvent>()
                .Where(e => e.TransactionType == "Deposit")
                .ToListAsync();

            // Assert
            Assert.Single(depositEvents);
            Assert.Equal("Deposit", depositEvents.First().TransactionType);
        }

        [Fact]
        public async Task Rollback_NoErrorsWhenNoMatchingEvents()
        {
            // Arrange: Ensure no events exist for a given day and account.
            var targetDay = new DateTime(2025, 3, 22);
            var handlerDay = new RollbackEventsByDayCommandHandler(_context);
            var commandDay = new RollbackEventsByDayCommand { Day = targetDay };

            var handlerAccount = new RollbackEventsByAccountCommandHandler(_context);
            var commandAccount = new RollbackEventsByAccountCommand { AccountId = 9999 };

            // Act
            var resultDay = await handlerDay.Handle(commandDay, CancellationToken.None);
            var resultAccount = await handlerAccount.Handle(commandAccount, CancellationToken.None);

            // Assert: Both commands should succeed even if no matching events were found.
            Assert.True(resultDay.IsSuccess);
            Assert.True(resultAccount.IsSuccess);
        }
    }
}
