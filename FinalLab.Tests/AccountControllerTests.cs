using FinalLab.Application.Handlers;
using FinalLab.Application.Queries;
using FinalLab.Domain.Entities;
using FinalLab.Infrastructure.Persistence;
using FinalLab.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FinalLab.Tests
{
    public class GetAccountBalanceSummaryHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsCorrectBalanceSummary()
        {
            // Arrange: 
            var accounts = new List<Account>
            {
                new Account { AccountId = 1, Balance = 1000 },
                new Account { AccountId = 2, Balance = 2000 }
            };

            var transactions = new List<Transaction>
            {
                new Transaction { AccountId = 1, Amount = 100, TransactionType = "Deposit" },
                new Transaction { AccountId = 1, Amount = 50, TransactionType = "Withdrawal" },
                new Transaction { AccountId = 2, Amount = 200, TransactionType = "Deposit" },
                new Transaction { AccountId = 2, Amount = 100, TransactionType = "Withdrawal" }
            };

            var accountRepoMock = new Mock<IAccountRepository>();
            accountRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(accounts);

            var transactionRepoMock = new Mock<ITransactionRepository>();
            transactionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(transactions);

            var handler = new GetAccountBalanceSummaryHandler(accountRepoMock.Object, transactionRepoMock.Object);
            var query = new GetBalanceSummaryQuery();

            // Act: 
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert: 
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var summary1 = result.First(x => x.AccountId == 1);
            Assert.Equal(100, summary1.TotalDeposits);
            Assert.Equal(50, summary1.TotalWithdrawals);
            Assert.Equal(1000, summary1.CurrentBalance);

            var summary2 = result.First(x => x.AccountId == 2);
            Assert.Equal(200, summary2.TotalDeposits);
            Assert.Equal(100, summary2.TotalWithdrawals);
            Assert.Equal(2000, summary2.CurrentBalance);
        }
    }

    public class GetCommonTransactionsHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsCommonTransactionsGroups()
        {
            // Arrange:
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestCommonTransactions")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Transactions.RemoveRange(context.Transactions);
                await context.SaveChangesAsync();

                
                
                
                var transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Id = 1, AccountId = 1, Amount = 100, TransactionType = "Deposit", Status = "Useless",
                        Details = "Also Useless", Timestamp = DateTime.UtcNow.AddHours(-1)
                    },
                    new Transaction
                    {
                        Id = 2, AccountId = 2, Amount = 100, TransactionType = "Deposit", Status = "Useless",
                        Details = "Also Useless", Timestamp = DateTime.UtcNow.AddHours(-2)
                    },
                    new Transaction
                    {
                        Id = 3, AccountId = 1, Amount = 50, TransactionType = "Withdrawal", Status = "Useless",
                        Details = "Also Useless", Timestamp = DateTime.UtcNow.AddHours(-3)
                    },
                    new Transaction
                    {
                        Id = 4, AccountId = 2, Amount = 50, TransactionType = "Withdrawal", Status = "Useless",
                        Details = "Also Useless", Timestamp = DateTime.UtcNow.AddHours(-1)
                    },
                    // transaction not common to both accounts
                    new Transaction
                    {
                        Id = 5, AccountId = 1, Amount = 200, TransactionType = "Deposit", Status = "Useless",
                        Details = "Also Useless", Timestamp = DateTime.UtcNow.AddHours(-6)
                    }
                };
                context.Transactions.AddRange(transactions);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var repository = new TransactionRepository(context);
                var handler = new GetCommonTransactionsHandler(repository);
                var query = new GetCommonTransactionsQuery(new List<long> { 1, 2 });

                // Act: 
                var result = await handler.Handle(query, CancellationToken.None);

                // Assert: 
                Assert.NotNull(result);
                Assert.Equal(4, result.Count);

                bool hasAmount100Group = result.Any(g => g.All(t => t.Amount == 100));
                bool hasAmount50Group = result.Any(g => g.All(t => t.Amount == 50));
                bool hasDepositGroup = result.Any(g => g.All(t => t.TransactionType == "Deposit"));
                bool hasWithdrawalGroup = result.Any(g => g.All(t => t.TransactionType == "Withdrawal"));

                Assert.True(hasAmount100Group, "Expected common group for amount 100");
                Assert.True(hasAmount50Group, "Expected common group for amount 50");
                Assert.True(hasDepositGroup, "Expected common group for Deposit transactions");
                Assert.True(hasWithdrawalGroup, "Expected common group for Withdrawal transactions");
            }
        }
    }
}