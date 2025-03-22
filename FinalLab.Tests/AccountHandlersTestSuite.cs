using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinalLab.Application.Commands.ChangeAccountStatus;
using FinalLab.Application.Commands.CreateAccount;
using FinalLab.Application.Queries;
using FinalLab.Common;
using FinalLab.Domain.Entities;
using FinalLab.Infrastructure.Persistence;
using FinalLab.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinalLab.Tests
{
    // This test suite uses your existing ApplicationDbContext.
    // Ensure ApplicationDbContext exposes a DbSet<Account> and is properly configured.
    public class CommandAndQueryHandlerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountRepository _repository;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public CommandAndQueryHandlerTests()
        {
            // Create a unique in-memory database instance for each test run.
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                        .Options;
            _context = new ApplicationDbContext(_options);
            _repository = new AccountRepository(_context);
        }

        public void Dispose()
        {
            // Clean up the in-memory database after each test.
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateAccountCommandHandler_ShouldCreateAccountSuccessfully()
        {
            // Arrange
            var handler = new CreateAccountCommandHandler(_repository);
            var command = new CreateAccountCommand
            {
                initialBalance = 200m
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.NotEqual(0, result.Value.AccountId); // EF should assign a non-zero ID
            Assert.Equal(200m, result.Value.Balance);
        }

        [Fact]
        public async Task ChangeAccountStatusCommandHandler_ShouldChangeStatusSuccessfully()
        {
            // Arrange
            // Seed an account into the database.
            var account = new Account
            {
                Balance = 150m,
                Status = "Active"
            };
            await _repository.AddAsync(account);
            await _repository.SaveAsync();

            var handler = new ChangeAccountStatusCommandHandler(_repository);
            var command = new ChangeAccountStatusCommand
            {
                accountId = account.AccountId,
                newStatus = "Inactive"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            // Reload the account from the database to verify the status change.
            var updatedAccount = (await _repository.GetAllAsync())
                                    .First(a => a.AccountId == account.AccountId);
            Assert.Equal("Inactive", updatedAccount.Status);
        }

        [Fact]
        public async Task ChangeAccountStatusCommandHandler_ShouldReturnFailure_WhenAccountNotFound()
        {
            // Arrange
            var handler = new ChangeAccountStatusCommandHandler(_repository);
            var command = new ChangeAccountStatusCommand
            {
                accountId = 999, // non-existent account
                newStatus = "Inactive"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Account not found", result.Error);
        }

        [Fact]
        public async Task GetBalanceQueryHandler_ShouldReturnBalanceSuccessfully()
        {
            // Arrange
            var account = new Account
            {
                Balance = 300m,
                Status = "Active"
            };
            await _repository.AddAsync(account);
            await _repository.SaveAsync();

            var handler = new GetBalanceQueryHandler(_repository);
            var query = new GetBalanceQuery
            {
                accountId = account.AccountId
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(300m, result.Value);
        }

        [Fact]
        public async Task GetBalanceQueryHandler_ShouldReturnFailure_WhenAccountNotFound()
        {
            // Arrange
            var handler = new GetBalanceQueryHandler(_repository);
            var query = new GetBalanceQuery
            {
                accountId = 999 // non-existent account
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Account not found", result.Error);
        }
    }
}
