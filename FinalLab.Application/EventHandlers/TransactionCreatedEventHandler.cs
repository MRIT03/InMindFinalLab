using FinalLab.Domain.Events;
using MediatR;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FinalLab.Application.EventHandlers
{
    public class TransactionCreatedEventHandler : INotificationHandler<TransactionCreatedEvent>
    {
        public async Task Handle(TransactionCreatedEvent notification, CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: "Transactions",
                type: ExchangeType.Fanout,
                durable: true,        
                autoDelete: false,
                arguments: null);

            var message = JsonSerializer.Serialize(notification);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublishAsync(exchange: "Transactions",
                routingKey: "",
                
                body: body);

            Console.WriteLine($"Published transaction event: {message}");

            await Task.CompletedTask;
        }
    }
}