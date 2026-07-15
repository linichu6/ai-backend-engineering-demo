
#nullable enable
using System.Threading.Tasks;
using StockApi.Models;

namespace StockApi.Providers
{
    public interface IStockProvider
    {
        /// <summary>
        /// Get the closing stock price for the request. If request.Date is null, the provider should use DateTime.UtcNow.Date.
        /// </summary>
        /// <param name="request">Stock price request containing ticker and optional date.</param>
        /// <returns>Stock price response for the effective date.</returns>
        Task<StockPriceResponse> GetStockPriceAsync(StockPriceRequest request, CancellationToken cancellationToken);
    }
}