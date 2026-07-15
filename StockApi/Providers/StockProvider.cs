#nullable enable
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StockApi.Models;
using StockApi.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StockApi.Providers
{
    public class StockProvider : IStockProvider
    {
        private readonly IStockRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<StockProvider> _logger;

        public StockProvider(IStockRepository repository,
                             IMemoryCache cache, 
                             ILogger<StockProvider> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <inheritdoc />
        public async Task<StockPriceResponse> GetStockPriceAsync(StockPriceRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var ticker = request.Ticker;
            if (ticker is null)
            {
                throw new ArgumentException("Ticker is required.", nameof(request.Ticker));
            }

            ticker = ticker.Trim();
            if (ticker.Length == 0)
            {
                throw new ArgumentException("Ticker is required.", nameof(request.Ticker));
            }

            var effectiveDate = request.Date?.Date ?? DateTime.UtcNow.Date;

            try
            {
                string cacheKey = $"stock:{request.Ticker}:{effectiveDate:yyyy-MM-dd}";

                if (_cache.TryGetValue(cacheKey, out StockPriceResponse? cached))
                {
                    _logger.LogInformation(
                        "Cache hit for {Ticker}",
                        request.Ticker);

                    return cached!;
                }

                var result = await _repository.GetStockPriceAsync(ticker, effectiveDate, cancellationToken).ConfigureAwait(false);

                if (result is null)
                {
                    _logger.LogWarning("Ticker '{Ticker}' not found for date {Date}.", ticker, effectiveDate.ToString("yyyy-MM-dd"));
                    throw new KeyNotFoundException($"Ticker '{ticker}' not found for date {effectiveDate:yyyy-MM-dd}.");
                }

                _cache.Set(
                        cacheKey,
                        result,
                        new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                        });

                return result;
            }
            catch (OperationCanceledException)
            {
                // Cancellation is cooperative; don't log as error.
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve stock price for {Ticker}", ticker);
                throw;
            }
        }
    }
}