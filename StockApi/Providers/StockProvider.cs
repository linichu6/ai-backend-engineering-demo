#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using StockApi.Models;
using StockApi.Repositories;

namespace StockApi.Providers
{
    public class StockProvider : IStockProvider
    {
        private readonly IStockRepository _repository;

        public StockProvider(IStockRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
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

            var result = await _repository.GetStockPriceAsync(ticker, effectiveDate, cancellationToken).ConfigureAwait(false);

            if (result is null)
            {
                throw new KeyNotFoundException($"Ticker '{ticker}' not found for date {effectiveDate:yyyy-MM-dd}.");
            }

            return result;
        }
    }
}