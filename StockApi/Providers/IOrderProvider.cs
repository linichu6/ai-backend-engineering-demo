#nullable enable
using System.Threading.Tasks;
using OrderApi.Models;

namespace OrderApi.Providers
{
    public interface IOrderProvider
    {
        /// <summary>
        /// Processes a PayPal payment order and publishes to Kafka.
        /// </summary>
        /// <param name="request">The order creation request.</param>
        /// <returns>The order response.</returns>
        Task<OrderResponse> CreatePayPalOrderAsync(CreateOrderRequest request);
    }
}