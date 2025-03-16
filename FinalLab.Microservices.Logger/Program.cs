using FinalLab.Domain.Entities;
using FinalLab.Domain.Events;
using FinalLab.Microservices.Logger.Contexts;
using FinalLab.Microservices.Logger.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=BankLoggingDb;Username=postgres;Password=postgres"));

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<LogEntryConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("localhost", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Configure the message to use the "logging" exchange
        cfg.Message<LogEntryCreatedEvent>(config =>
        {
            config.SetEntityName("logging");
        });

        // Bind a receive endpoint to the "logging" exchange
        cfg.ReceiveEndpoint("logging_queue", e =>
        {
            e.Bind("logging", s =>
            {
                s.ExchangeType = "fanout";
            });
            e.ConfigureConsumer<LogEntryConsumer>(ctx);
        });

        cfg.Publish<LogEntryCreatedEvent>(p =>
        {
            p.ExchangeType = "fanout";
        });
    });
});



var app = builder.Build();
app.UseAuthorization();
app.MapControllers();
app.Run();
