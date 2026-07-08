#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StockApi.Models;
using StockApi.Services;

namespace StockApi.Providers
{
    public class PaymentProvider : IPaymentProvider
    {
        private readonly IKafkaProducerService _kafkaProducerService;
        private readonly ILogger<PaymentProvider> _logger;

        public PaymentProvider(IKafkaProducerService kafkaProducerService, ILogger<PaymentProvider> logger)
        {
            _kafkaProducerService = kafkaProducerService ?? throw new ArgumentNullException(nameof(kafkaProducerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ValidatePaymentRequest(request);

            // Create Kafka message
            var kafkaMessage = new PaymentKafkaMessage
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
                Currency = request.Currency,
                PayerEmail = request.PayerEmail,
                Description = request.Description,
                Status = "PENDING",
                CreatedAt = request.CreatedAt
            };

            // Publish to Kafka
            await _kafkaProducerService.PublishPaymentMessageAsync(kafkaMessage).ConfigureAwait(false);

            _logger.LogInformation(
                "Payment processed and published to Kafka. OrderId: {OrderId}, Amount: {Amount} {Currency}",
                request.OrderId,
                request.Amount,
                request.Currency);

            // Return response
            return new PaymentResponse
            {
                OrderId = request.OrderId,
                Status = "PROCESSING",
                Amount = request.Amount,
                Currency = request.Currency,
                PayerEmail = request.PayerEmail,
                CreatedAt = request.CreatedAt,
                ProcessedAt = DateTime.UtcNow
            };
        }

        private static void ValidatePaymentRequest(PaymentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.OrderId))
            {
                throw new ArgumentException("OrderId is required.", nameof(request.OrderId));
            }

            if (request.Amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0.", nameof(request.Amount));
            }

            if (string.IsNullOrWhiteSpace(request.PayerEmail))
            {
                throw new ArgumentException("PayerEmail is required.", nameof(request.PayerEmail));
            }

            if (string.IsNullOrWhiteSpace(request.Currency))
            {
                throw new ArgumentException("Currency is required.", nameof(request.Currency));
            }
        }
    }
}