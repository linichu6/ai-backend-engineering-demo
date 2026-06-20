using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StockApi.Models;
using StockApi.Providers;
using StockApi.Repositories;

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
