//csharp StockApi/Repositories/StockRepository.cs
#nullable enable
using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using StockApi.Models;

namespace StockApi.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly HttpClient _httpClient;

        public StockRepository(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClient.BaseAddress = new Uri("https://query1.finance.yahoo.com/");
        }

        /// <inheritdoc />
        public async Task<StockPriceResponse?> GetStockPriceAsync(string ticker, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(ticker))
            {
                throw new ArgumentException("Ticker is required.", nameof(ticker));
            }

            // Convert date to Unix timestamps (seconds)
            var period1Utc = new DateTimeOffset(date.Date, TimeSpan.Zero).ToUnixTimeSeconds();
            var period2Utc = new DateTimeOffset(date.Date.AddDays(1), TimeSpan.Zero).ToUnixTimeSeconds();

            var requestUri = $"v8/finance/chart/{Uri.EscapeDataString(ticker)}?period1={period1Utc}&period2={period2Utc}&interval=1d";

            using var response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                // treat non-success (404 etc.) as not found
                return null;
            }

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(content);

                // Check for chart.error
                if (doc.RootElement.TryGetProperty("chart", out var chartElement))
                {
                    if (chartElement.TryGetProperty("error", out var errorElement) && errorElement.ValueKind != JsonValueKind.Null)
                    {
                        // API returned an error for the ticker
                        return null;
                    }

                    if (chartElement.TryGetProperty("result", out var resultArray) && resultArray.ValueKind == JsonValueKind.Array && resultArray.GetArrayLength() > 0)
                    {
                        var result = resultArray[0];

                        // Navigate to indicators.quote[0].close
                        if (result.TryGetProperty("indicators", out var indicators) &&
                            indicators.TryGetProperty("quote", out var quoteArray) && quoteArray.ValueKind == JsonValueKind.Array && quoteArray.GetArrayLength() > 0)
                        {
                            var quote = quoteArray[0];

                            if (quote.TryGetProperty("close", out var closeArray) && closeArray.ValueKind == JsonValueKind.Array && closeArray.GetArrayLength() > 0)
                            {
                                var closeElement = closeArray[0];

                                if (closeElement.ValueKind == JsonValueKind.Number && closeElement.TryGetDecimal(out var closeDecimal))
                                {
                                    return new StockPriceResponse
                                    {
                                        Ticker = ticker.ToUpperInvariant(),
                                        Date = date.Date,
                                        ClosePrice = decimal.Round(closeDecimal, 2)
                                    };
                                }

                                // value may be null
                                if (closeElement.ValueKind == JsonValueKind.Null)
                                {
                                    return null;
                                }
                            }
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // parsing error - treat as not found/unavailable
                return null;
            }

            return null;
        }
    }
}