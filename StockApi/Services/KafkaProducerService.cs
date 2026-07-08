#nullable enable
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using StockApi.Models;

namespace StockApi.Services
{
    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;
        private const string PaymentTopic = "payment-events";

        public KafkaProducerService(IProducer<string, string> producer, ILogger<KafkaProducerService> logger)
        {
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PublishPaymentMessageAsync(PaymentKafkaMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            try
            {
                var messageJson = JsonSerializer.Serialize(message);
                var kafkaMessage = new Message<string, string>
                {
                    Key = message.OrderId,
                    Value = messageJson
                };

                var result = await _producer.ProduceAsync(PaymentTopic, kafkaMessage).ConfigureAwait(false);

                _logger.LogInformation(
                    "Payment message published to Kafka. OrderId: {OrderId}, Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                    message.OrderId,
                    result.Topic,
                    result.Partition,
                    result.Offset);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to publish payment message to Kafka. OrderId: {OrderId}, Error: {Error}",
                    message.OrderId,
                    ex.Error.Reason);
                throw;
            }
        }
    }
}