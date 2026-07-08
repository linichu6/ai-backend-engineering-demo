#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Models;
using OrderApi.Providers;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderProvider _provider;

        public OrderController(IOrderProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Creates a new PayPal order and publishes it to Kafka for processing.
        /// This is a sample implementation and does not actually process real payments.
        /// </summary>
        /// <param name="request">The order creation request containing payment details.</param>
        /// <returns>Order response with processing status.</returns>
        [HttpPost("paypal")]
        public async Task<IActionResult> CreatePayPalOrder([FromBody] CreateOrderRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required." });
            }

            try
            {
                var response = await _provider.CreatePayPalOrderAsync(request).ConfigureAwait(false);
                return Accepted(response);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(new { message = ae.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}