using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FinalLab.Microservices.Logger.Services;

public class RabbitMQConsumerService : BackgroundService
{
    private readonly HttpClient _httpClient;
    private IConnection _connection;
    private IChannel _channel;

    public RabbitMQConsumerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = await factory.CreateConnectionAsync(); 
        _channel = await _connection.CreateChannelAsync();
        await _channel.QueueDeclareAsync(queue: "log_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

        Console.WriteLine("RabbitMQ Consumer Service started.");
        
        await base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
        {
            Console.WriteLine("RabbitMQ channel is not initialized.");
            return Task.CompletedTask;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                var content = new StringContent(message, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("http://localhost:5041/api/logs", content);

                Console.WriteLine($"Sent log to API. Response: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error forwarding log to API: {ex.Message}");
            }
        };

        _channel.BasicConsumeAsync(queue: "log_queue", autoAck: true, consumer: consumer);

        Console.WriteLine("RabbitMQ Consumer Service is listening...");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _channel?.CloseAsync();
        await _connection?.CloseAsync();
        Console.WriteLine("RabbitMQ Consumer Service stopped.");
        await base.StopAsync(cancellationToken);
    }
}
