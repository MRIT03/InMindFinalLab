using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinalLab.Application.Commands;
using FinalLab.Application.Commands.DepositTransaction;
using FinalLab.Application.Commands.WithdrawlTransaction;
using FinalLab.Common;
using FinalLab.Domain.Entities;
using FinalLab.Infrastructure.Persistence;
using FinalLab.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FinalLab.Tests
{
    public class TransactionCommandHandlerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountRepository _repository;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public TransactionCommandHandlerTests()
        {
            // Create a unique in-memory database for isolation.
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(_options);
            _repository = new AccountRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Deposit Transaction Tests

        [Fact]
        public async Task DepositTransactionCommandHandler_ShouldDepositValidAmountAndUpdateBalance()
        {
            // Arrange: Seed an account with an initial balance.
            var account = new Account { Balance = 100, Status = "Active" };
            await _repository.AddAsync(account);
            await _repository.SaveAsync();

            // Set up mediator mock to simulate the creation of a deposit transaction.
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateTransactionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Transaction
                {
                    Id = 1,
                    AccountId = account.AccountId,
                    Amount = 50,
                    TransactionType = "Deposit",
                    Details = "Deposit transaction",
                    Status = "In progress"
                });

            var handler = new DepositTransactionCommandHandler(_repository, mediatorMock.Object);
            var command = new DepositTransactionCommand { AccountId = account.AccountId, Amount = 50 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert: Verify a successful result and that the account balance increased.
            Assert.True(result.IsSuccess);
            var updatedAccount = (await _repository.GetAllAsync())
                                    .First(a => a.AccountId == account.AccountId);
            Assert.Equal(150, updatedAccount.Balance);
            Assert.NotNull(result.Value);
            Assert.Equal("Deposit", result.Value.TransactionType);
        }

        [Fact]
        public async Task DepositTransactionCommandHandler_ShouldReturnFailureForInvalidAmount()
        {
            // Arrange: No need to seed an account when amount is invalid.
            var mediatorMock = new Mock<IMediator>();
            var handler = new DepositTransactionCommandHandler(_repository, mediatorMock.Object);
            var command = new DepositTransactionCommand { AccountId = 1, Amount = 0 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert: Verify failure message for non-positive deposit amount.
            Assert.False(result.IsSuccess);
            Assert.Equal("Deposit transaction amount must be greater than 0", result.Error);
        }

        [Fact]
        public async Task DepositTransactionCommandHandler_ShouldReturnFailureWhenAccountNotFound()
        {
            // Arrange: Use a non-existent account Id.
            var mediatorMock = new Mock<IMediator>();
            var handler = new DepositTransactionCommandHandler(_repository, mediatorMock.Object);
            var command = new DepositTransactionCommand { AccountId = 999, Amount = 50 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Account not found", result.Error);
        }

        #endregion

        #region Withdrawal Transaction Tests

        [Fact]
        public async Task WithdrawlTransactionCommandHandler_ShouldWithdrawValidAmountAndUpdateBalance()
        {
            // Arrange: Seed an account with an initial balance.
            var account = new Account { Balance = 100, Status = "Active" };
            await _repository.AddAsync(account);
            await _repository.SaveAsync();

            // Set up mediator mock to simulate the creation of a withdrawal transaction.
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateTransactionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Transaction
                {
                    Id = 2,
                    AccountId = account.AccountId,
                    Amount = 40,
                    TransactionType = "Withdrawl",
                    Details = "Withdrawl transaction",
                    Status = "Pending"
                });

            var handler = new WithdrawlTransactionCommandHandler(mediatorMock.Object, _repository);
            var command = new WithdrawlTransactionCommand { accountId = account.AccountId, amount = 40 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert: Verify success and that the account balance has been decreased.
            Assert.True(result.IsSuccess);
            var updatedAccount = (await _repository.GetAllAsync())
                                    .First(a => a.AccountId == account.AccountId);
            Assert.Equal(60, updatedAccount.Balance);
            Assert.NotNull(result.Value);
            Assert.Equal("Withdrawl", result.Value.TransactionType);
        }

        [Fact]
        public async Task WithdrawlTransactionCommandHandler_ShouldReturnFailureForInvalidAmount()
        {
            // Arrange: Test an invalid withdrawal amount (0 or negative).
            var mediatorMock = new Mock<IMediator>();
            var handler = new WithdrawlTransactionCommandHandler(mediatorMock.Object, _repository);
            var command = new WithdrawlTransactionCommand { accountId = 1, amount = 0 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Withdrawl transaction amount must be greater than 0", result.Error);
        }

        [Fact]
        public async Task WithdrawlTransactionCommandHandler_ShouldReturnFailureForInsufficientFunds()
        {
            // Arrange: Seed an account with insufficient balance.
            var account = new Account { Balance = 30, Status = "Active" };
            await _repository.AddAsync(account);
            await _repository.SaveAsync();

            var mediatorMock = new Mock<IMediator>();
            var handler = new WithdrawlTransactionCommandHandler(mediatorMock.Object, _repository);
            var command = new WithdrawlTransactionCommand { accountId = account.AccountId, amount = 50 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Insufficient funds", result.Error);
        }

        [Fact]
        public async Task WithdrawlTransactionCommandHandler_ShouldReturnFailureWhenAccountNotFound()
        {
            // Arrange: Use a non-existent account Id.
            var mediatorMock = new Mock<IMediator>();
            var handler = new WithdrawlTransactionCommandHandler(mediatorMock.Object, _repository);
            var command = new WithdrawlTransactionCommand { accountId = 999, amount = 50 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Account not found", result.Error);
        }

        #endregion
    }
}
