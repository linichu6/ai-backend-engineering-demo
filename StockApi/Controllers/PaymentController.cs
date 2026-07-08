#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StockApi.Models;
using StockApi.Providers;

namespace StockApi.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentProvider _provider;

        public PaymentController(IPaymentProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Accepts a PayPal payment request and publishes it to Kafka for processing.
        /// This is a sample implementation and does not actually process real payments.
        /// </summary>
        /// <param name="request">The payment request containing order details.</param>
        /// <returns>Payment response with processing status.</returns>
        [HttpPost("paypal")]
        public async Task<IActionResult> ProcessPayPalPayment([FromBody] PaymentRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required." });
            }

            try
            {
                var response = await _provider.ProcessPaymentAsync(request).ConfigureAwait(false);
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