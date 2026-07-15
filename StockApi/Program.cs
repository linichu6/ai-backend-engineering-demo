using StockApi.Models;
using StockApi.Providers;
using StockApi.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency injection
builder.Services.Configure<AlphaVantageOptions>(
    builder.Configuration.GetSection("AlphaVantage"));
builder.Services.AddHttpClient<IStockRepository, AVStockRepository>();
builder.Services.AddScoped<IStockProvider, StockProvider>();
builder.Services.AddMemoryCache();

// log
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/log-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
