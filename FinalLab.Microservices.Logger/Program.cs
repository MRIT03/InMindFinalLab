using FinalLab.Microservices.Logger.Contexts;
using FinalLab.Microservices.Logger.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=BankLoggingDb;Username=postgres;Password=postgres"));

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<RabbitMQConsumerService>();

var app = builder.Build();
app.UseAuthorization();
app.MapControllers();
app.Run();
