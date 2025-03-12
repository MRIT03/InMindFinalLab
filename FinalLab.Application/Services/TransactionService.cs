using FinalLab.Domain.Entities;
using FinalLab.Domain.Events;
using MediatR;
using System.Threading.Tasks;
using FinalLab.Application.Commands;

namespace FinalLab.Application.Services
{
    public class TransactionService
    {
        private readonly IMediator _mediator;

        public TransactionService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task CreateTransactionAsync(Transaction transaction)
        {
            // Here, you would typically call the repository to save the transaction to the database.
            CreateTransactionCommand command = new CreateTransactionCommand()
            {
                AccountId = transaction.AccountId,
                Amount = transaction.Amount,
                status = transaction.Status,
                TransactionType = transaction.TransactionType,
                Details = transaction.Details,
            };
            _mediator.Send(command).Wait();
            // Publish event after transaction creation
            await _mediator.Publish(new TransactionCreatedEvent(transaction.Id, transaction.AccountId, transaction.Amount, transaction.TransactionType));
        }
    }
}