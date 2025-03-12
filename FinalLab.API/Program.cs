using System.Reflection;
using FinalLab.Application.EventHandlers;
using FinalLab.Application.Services;
using FinalLab.Domain.Entities;
using FinalLab.Domain.Events;
using FinalLab.Infrastructure.Persistence;

using FinalLab.Persistence.Repositories;
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


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// RabbitMQ Connection
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory { HostName = "localhost"};
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});


builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();

builder.Services.AddScoped<TransactionService>();


builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TransactionCreatedEventHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));


builder.Services.AddScoped<INotificationHandler<TransactionCreatedEvent>, TransactionCreatedEventHandler>();


var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<Transaction>("Transactions");

builder.Services.AddControllers().AddOData(options =>
    options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100).AddRouteComponents("odata", modelBuilder.GetEdmModel()));


var app = builder.Build();

app.UseRouting();

app.MapGet("/", () => "Banking System API is Running...");

app.Run();