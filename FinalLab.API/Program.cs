using System.Globalization;
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
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OData.ModelBuilder;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Localization configuration moved here, before Build()
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new List<CultureInfo>
    {
        new CultureInfo("en"),
        new CultureInfo("fr")
    };

    // Set French as the default culture
    options.DefaultRequestCulture = new RequestCulture("fr");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Controllers & Views with localization
builder.Services.AddControllersWithViews()
    .AddDataAnnotationsLocalization();

// Logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Database context
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

        cfg.Message<LogEntryCreatedEvent>(config =>
        {
            config.SetEntityName("logging");
        });

        cfg.Publish<LogEntryCreatedEvent>(p =>
        {
            p.ExchangeType = "fanout";
        });
    });
});

// Repositories and Services
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

// MediatR Handlers
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CreateTransactionHandler).Assembly));

builder.Services.AddScoped<INotificationHandler<TransactionCreatedEvent>, TransactionCreatedEventHandler>();
builder.Services.AddScoped<INotificationHandler<MoneyTransferredEvent>, MoneyTransferredEventHandler>();
builder.Services.AddScoped<INotificationHandler<AccountCreatedEvent>, AccountCreatedEventHandler>();
builder.Services.AddScoped<INotificationHandler<AccountModifiedEvent>, AccountModifiedEventHandler>();

// OData configuration
var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<Transaction>("_TransactionRepository");

builder.Services.AddControllers().AddOData(options =>
    options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100).AddRouteComponents("odata", modelBuilder.GetEdmModel()));

var app = builder.Build();
var locOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);
app.UseRequestLogging();
app.UseRouting();

app.MapGet("/", () => "Banking System API is Running...");
app.MapControllers();
app.Run();
