#nullable enable
using System.Threading.Tasks;
using StockApi.Models;

namespace StockApi.Services
{
    public interface IKafkaProducerService
    {
        /// <summary>
        /// Publishes a payment message to Kafka.
        /// </summary>
        /// <param name="message">The payment Kafka message to publish.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task PublishPaymentMessageAsync(PaymentKafkaMessage message);
    }
}