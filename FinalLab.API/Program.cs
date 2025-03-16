using System.Reflection;
using FinalLab.API.Middleware;
using FinalLab.Application.EventHandlers;
using FinalLab.Application.Querries;
using FinalLab.Application.Services;
using FinalLab.Domain.Entities;
using FinalLab.Domain.Events;
using FinalLab.Domain.Events.DomainEvents;
using FinalLab.Infrastructure.Persistence;

using FinalLab.Persistence.Repositories;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OData.ModelBuilder;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();  // Logs to the console
    logging.AddDebug();    // Logs to the debug output
});

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// RabbitMQ Connection
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Configure the exchange name for LogEntryCreatedEvent to "logging"
        cfg.Message<LogEntryCreatedEvent>(config =>
        {
            config.SetEntityName("logging");
        });

        // Ensure published messages use a fanout exchange
        cfg.Publish<LogEntryCreatedEvent>(p =>
        {
            p.ExchangeType = "fanout";
        });
    });
});

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();

builder.Services.AddScoped<ITransactionService, TransactionService>();


builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CreateTransactionHandler).Assembly));


builder.Services.AddScoped<INotificationHandler<TransactionCreatedEvent>, TransactionCreatedEventHandler>();
builder.Services.AddScoped<INotificationHandler<MoneyTransferredEvent>, MoneyTransferredEventHandler>();
builder.Services.AddScoped<INotificationHandler<AccountCreatedEvent>, AccountCreatedEventHandler>();
builder.Services.AddScoped<INotificationHandler<AccountModifiedEvent>, AccountModifiedEventHandler>();

var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<Transaction>("_TransactionRepository");

builder.Services.AddControllers().AddOData(options =>
    options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100).AddRouteComponents("odata", modelBuilder.GetEdmModel()));


var app = builder.Build();
app.UseRequestLogging();
app.UseRouting();

app.MapGet("/", () => "Banking System API is Running...");
app.MapControllers();
app.Run();