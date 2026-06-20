
#nullable enable
using System;
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
        public async Task<StockPriceResponse> GetStockPriceAsync(StockPriceRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var ticker = request.Ticker?.Trim();
            if (string.IsNullOrWhiteSpace(ticker))
            {
                throw new ArgumentException("Ticker is required.", nameof(request.Ticker));
            }

            var effectiveDate = request.Date?.Date ?? DateTime.UtcNow.Date;

            var result = await _repository.GetStockPriceAsync(ticker, effectiveDate).ConfigureAwait(false);

            if (result is null)
            {
                throw new KeyNotFoundException($"Ticker '{ticker}' not found for date {effectiveDate:yyyy-MM-dd}.");
            }

            return result;
        }
    }
}