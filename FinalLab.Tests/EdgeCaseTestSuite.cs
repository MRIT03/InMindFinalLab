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
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FinalLab.Tests
{
    public class EdgeCaseTransactionTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountRepository _repository;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public EdgeCaseTransactionTests()
        {
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

        #region Deposit Transaction Edge Cases

        [Fact]
        public async Task DepositTransaction_InvalidNegativeAmount_ReturnsFailure()
        {
            // Arrange
            var mediatorMock = new Mock<MediatR.IMediator>();
            var handler = new DepositTransactionCommandHandler(_repository, mediatorMock.Object);
            var command = new DepositTransactionCommand { AccountId = 1, Amount = -10m };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Deposit transaction amount must be greater than 0", result.Error);
        }

        [Fact]
        public async Task DepositTransaction_ZeroAmount_ReturnsFailure()
        {
            // Arrange
            var mediatorMock = new Mock<MediatR.IMediator>();
            var handler = new DepositTransactionCommandHandler(_repository, mediatorMock.Object);
            var command = new DepositTransactionCommand { AccountId = 1, Amount = 0m };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Deposit transaction amount must be greater than 0", result.Error);
        }

        [Fact]
        public async Task DepositTransaction_NonExistentAccount_ReturnsFailure()
        {
            // Arrange
            var mediatorMock = new Mock<MediatR.IMediator>();
            var handler = new DepositTransactionCommandHandler(_repository, mediatorMock.Object);
            var command = new DepositTransactionCommand { AccountId = 999, Amount = 50m };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Account not found", result.Error);
        }

        [Fact]
        public async Task DepositTransaction_MaximumAmount_Succeeds()
        {
            // Arrange: 
            var mediatorMock = new Mock<MediatR.IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateTransactionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Transaction
                {
                    Id = 1,
                    AccountId = 1,
                    Amount = decimal.MaxValue,
                    TransactionType = "Deposit",
                    Details = "Deposit transaction",
                    Status = "In progress",
                    Timestamp = DateTime.UtcNow
                });

            var account = new Account { Balance = 0m, Status = "Active" };
            await _repository.AddAsync(account);
            await _repository.SaveAsync();

            var handler = new DepositTransactionCommandHandler(_repository, mediatorMock.Object);
            var command = new DepositTransactionCommand { AccountId = account.AccountId, Amount = decimal.MaxValue };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert:
            Assert.True(result.IsSuccess);
            var updatedAccount = (await _repository.GetAllAsync()).First(a => a.AccountId == account.AccountId);
            Assert.Equal(decimal.MaxValue, updatedAccount.Balance);
        }

        [Fact]
        public async Task DepositTransaction_MinimumPositiveAmount_Succeeds()
        {
            // Arrange:
            var mediatorMock = new Mock<MediatR.IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateTransactionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Transaction
                {
                    Id = 1,
                    AccountId = 1,
                    Amount = 0.01m,
                    TransactionType = "Deposit",
                    Details = "Deposit transaction",
                    Status = "In progress",
                    Timestamp = DateTime.UtcNow
                });

            var account = new Account { Balance = 0m, Status = "Active" };
            await _repository.AddAsync(account);
            await _repository.SaveAsync();

            var handler = new DepositTransactionCommandHandler(_repository, mediatorMock.Object);
            var command = new DepositTransactionCommand { AccountId = account.AccountId, Amount = 0.01m };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert: 
            Assert.True(result.IsSuccess);
            var updatedAccount = (await _repository.GetAllAsync()).First(a => a.AccountId == account.AccountId);
            Assert.Equal(0.01m, updatedAccount.Balance);
        }

        #endregion

        #region Withdrawal Transaction Edge Cases

        [Fact]
        public async Task WithdrawlTransaction_InvalidNegativeAmount_ReturnsFailure()
        {
            // Arrange
            var mediatorMock = new Mock<MediatR.IMediator>();
            var handler = new WithdrawlTransactionCommandHandler(mediatorMock.Object, _repository);
            var command = new WithdrawlTransactionCommand { accountId = 1, amount = -20m };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Withdrawl transaction amount must be greater than 0", result.Error);
        }

        [Fact]
        public async Task WithdrawlTransaction_ZeroAmount_ReturnsFailure()
        {
            // Arrange
            var mediatorMock = new Mock<MediatR.IMediator>();
            var handler = new WithdrawlTransactionCommandHandler(mediatorMock.Object, _repository);
            var command = new WithdrawlTransactionCommand { accountId = 1, amount = 0m };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Withdrawl transaction amount must be greater than 0", result.Error);
        }

        [Fact]
        public async Task WithdrawlTransaction_NonExistentAccount_ReturnsFailure()
        {
            // Arrange
            var mediatorMock = new Mock<MediatR.IMediator>();
            var handler = new WithdrawlTransactionCommandHandler(mediatorMock.Object, _repository);
            var command = new WithdrawlTransactionCommand { accountId = 999, amount = 50m };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Account not found", result.Error);
        }

        [Fact]
        public async Task WithdrawlTransaction_MaximumAmount_Succeeds()
        {
            // Arrange: 
            var account = new Account { Balance = decimal.MaxValue, Status = "Active" };
            await _repository.AddAsync(account);
            await _repository.SaveAsync();

            var mediatorMock = new Mock<MediatR.IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateTransactionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Transaction
                {
                    Id = 1,
                    AccountId = account.AccountId,
                    Amount = decimal.MaxValue,
                    TransactionType = "Withdrawl",
                    Details = "Withdrawl transaction",
                    Status = "Pending",
                    Timestamp = DateTime.UtcNow
                });

            var handler = new WithdrawlTransactionCommandHandler(mediatorMock.Object, _repository);
            var command = new WithdrawlTransactionCommand { accountId = account.AccountId, amount = decimal.MaxValue };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert:
            Assert.True(result.IsSuccess);
            var updatedAccount = (await _repository.GetAllAsync()).First(a => a.AccountId == account.AccountId);
            Assert.Equal(0m, updatedAccount.Balance);
        }

        [Fact]
        public async Task WithdrawlTransaction_MinimumPositiveAmount_Succeeds()
        {
            // Arrange:
            var account = new Account { Balance = 0.02m, Status = "Active" };
            await _repository.AddAsync(account);
            await _repository.SaveAsync();

            var mediatorMock = new Mock<MediatR.IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateTransactionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Transaction
                {
                    Id = 1,
                    AccountId = account.AccountId,
                    Amount = 0.01m,
                    TransactionType = "Withdrawl",
                    Details = "Withdrawl transaction",
                    Status = "Pending",
                    Timestamp = DateTime.UtcNow
                });

            var handler = new WithdrawlTransactionCommandHandler(mediatorMock.Object, _repository);
            var command = new WithdrawlTransactionCommand { accountId = account.AccountId, amount = 0.01m };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert:
            Assert.True(result.IsSuccess);
            var updatedAccount = (await _repository.GetAllAsync()).First(a => a.AccountId == account.AccountId);
            Assert.Equal(0.01m, updatedAccount.Balance);
        }

        #endregion
    }
}
