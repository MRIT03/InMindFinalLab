using System;
using System.Threading;
using System.Threading.Tasks;
using FinalLab.Domain.Events;
using MassTransit;
using MediatR;

namespace FinalLab.Application.EventHandlers
{
    public class TransactionCreatedEventHandler : INotificationHandler<TransactionCreatedEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public TransactionCreatedEventHandler(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Handle(TransactionCreatedEvent notification, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish(notification, cancellationToken);
        }
    }
}