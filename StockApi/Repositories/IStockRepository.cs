//csharp StockApi/Repositories/IStockRepository.cs
#nullable enable
using System;
using System.Threading.Tasks;
using StockApi.Models;

namespace StockApi.Repositories
{
    public interface IStockRepository
    {
        /// <summary>
        /// Retrieves the closing price for the provided ticker and date. Returns null if the ticker or data is not found.
        /// </summary>
        /// <param name="ticker">Ticker symbol (e.g., AAPL).</param>
        /// <param name="date">Date (UTC date) for which to retrieve the closing price.</param>
        /// <returns>StockPriceResponse or null when data isn't available.</returns>
        Task<StockPriceResponse?> GetStockPriceAsync(string ticker, DateTime date, CancellationToken cancellationToken);
    }
}