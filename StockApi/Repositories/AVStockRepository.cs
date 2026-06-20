using Microsoft.Extensions.Options;
using StockApi.Models;
using System.Net.Http;
using System.Text.Json;

namespace StockApi.Repositories;

public class AVStockRepository : IStockRepository
{
    private readonly HttpClient _httpClient;
    private readonly AlphaVantageOptions _options;

    public AVStockRepository(
    HttpClient httpClient,
    IOptions<AlphaVantageOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<StockPriceResponse> GetStockPriceAsync(
    string ticker,
    DateTime date)
    {
        var url =
            $"https://www.alphavantage.co/query" +
            $"?function=TIME_SERIES_DAILY" +
            $"&symbol={ticker}" +
            $"&apikey={_options.ApiKey}";

        var response =
            await _httpClient.GetStringAsync(url);

        using var document =
            JsonDocument.Parse(response);

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

        return new StockPriceResponse
        {
            Ticker = ticker.ToUpper(),
            Date = actualDate.Date,
            ClosePrice = closePrice
        };
    }
}


