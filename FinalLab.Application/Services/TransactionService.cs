using FinalLab.Domain.Entities;
using FinalLab.Domain.Events;
using MediatR;
using System.Threading.Tasks;
using FinalLab.Application.Commands;
using Microsoft.Extensions.Logging;

namespace FinalLab.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(IMediator mediator, ILogger<TransactionService> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            _logger.LogInformation("Creating transaction: {@Transaction}", transaction);
            CreateTransactionCommand command = new CreateTransactionCommand()
            {
                AccountId = transaction.AccountId,
                Amount = transaction.Amount,
                status = transaction.Status,
                TransactionType = transaction.TransactionType,
                Details = transaction.Details,
            };
            var result = await _mediator.Send(command);
            // Publish event after transaction creation
            await _mediator.Publish(new TransactionCreatedEvent(transaction.Id, transaction.AccountId, transaction.Amount, transaction.TransactionType));
            _logger.LogInformation("Transaction event published: TransactionId={TransactionId}, FromAccountId={FromAccountId}, Amount={Amount}",
                transaction.Id, transaction.AccountId, transaction.Amount);
            return result;
        }
    }
}