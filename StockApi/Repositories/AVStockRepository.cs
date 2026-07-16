using Microsoft.Extensions.Options;
using StockApi.Models;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace StockApi.Repositories;

public class AVStockRepository : IStockRepository
{
    private readonly HttpClient _httpClient;
    private readonly AlphaVantageOptions _options;
    private readonly ILogger<AVStockRepository> _logger;
    private readonly string _apiKey;

    public AVStockRepository(
        HttpClient httpClient,
        IOptions<AlphaVantageOptions> options,
        ILogger<AVStockRepository> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _apiKey = options.Value.ApiKey; // populated from appsettings, environment, or user-secrets (dev)
    }

    public async Task<StockPriceResponse?> GetStockPriceAsync(
        string ticker,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"https://www.alphavantage.co/query" +
            $"?function=TIME_SERIES_DAILY" +
            $"&symbol={ticker}" +
            $"&apikey={_options.ApiKey}";

        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        using var document =
            JsonDocument.Parse(responseString);

        var root = document.RootElement;

        if (root.TryGetProperty("Error Message", out _))
        {
            throw new KeyNotFoundException(
                $"Ticker '{ticker}' not found.");
        }

        if (!root.TryGetProperty(
                "Time Series (Daily)",
                out var timeSeries))
        {
            throw new Exception(
                "Unexpected response from Alpha Vantage.");
        }

        var requestedDate =
            date.ToString("yyyy-MM-dd");

        var availableDates = timeSeries
            .EnumerateObject()
            .Select(x => DateTime.Parse(x.Name))
            .OrderByDescending(x => x)
            .ToList();

        var actualDate = availableDates
            .FirstOrDefault(d => d <= date);

        if (actualDate == default)
        {
            throw new KeyNotFoundException(
                $"No historical data found for {ticker}.");
        }

        var dayData =
            timeSeries.GetProperty(
                actualDate.ToString("yyyy-MM-dd"));

        var closePrice =
            decimal.Parse(
                dayData.GetProperty("4. close")
                       .GetString()!);

        _logger.LogInformation(
            "Ticker '{Ticker}' found for date {Date}: Close price {ClosePrice}.",
            ticker.ToUpper(),
            actualDate.Date.ToString("yyyy-MM-dd"),
            closePrice);

        return new StockPriceResponse
        {
            Ticker = ticker.ToUpper(),
            Date = actualDate.Date,
            ClosePrice = closePrice
        };
    }
}


