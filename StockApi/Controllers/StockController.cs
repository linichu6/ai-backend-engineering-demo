//csharp StockApi/Controllers/StockController.cs
#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StockApi.Models;
using StockApi.Providers;

namespace StockApi.Controllers
{
    [ApiController]
    [Route("api/stocks")]
    public class StockController : ControllerBase
    {
        private readonly IStockProvider _provider;

        public StockController(IStockProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Get the closing price for a stock ticker on a given date. If date is not provided, current UTC date is used.
        /// </summary>
        /// <param name="ticker">Ticker symbol (required).</param>
        /// <param name="date">Optional date in YYYY-MM-DD format.</param>
        /// <returns>Stock price information or appropriate HTTP error.</returns>
        [HttpGet("price")]
        public async Task<IActionResult> GetPrice([FromQuery] string? ticker, [FromQuery] DateTime? date)
        {
            if (string.IsNullOrWhiteSpace(ticker))
            {
                return BadRequest(new { message = "Ticker is required." });
            }

            var request = new StockPriceRequest
            {
                Ticker = ticker,
                Date = date
            };

            try
            {
                var response = await _provider.GetStockPriceAsync(request).ConfigureAwait(false);
                return Ok(response);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(new { message = ae.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Ticker not found." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}