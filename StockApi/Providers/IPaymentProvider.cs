#nullable enable
using System.Threading.Tasks;
using StockApi.Models;

namespace StockApi.Providers
{
    public interface IPaymentProvider
    {
        /// <summary>
        /// Processes a PayPal payment request and publishes to Kafka.
        /// </summary>
        /// <param name="request">The payment request.</param>
        /// <returns>The payment response.</returns>
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);
    }
}