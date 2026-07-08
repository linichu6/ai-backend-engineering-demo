#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrderApi.Models;
using OrderApi.Services;

namespace OrderApi.Providers
{
    public class OrderProvider : IOrderProvider
    {
        private readonly IKafkaProducerService _kafkaProducerService;
        private readonly ILogger<OrderProvider> _logger;

        public OrderProvider(IKafkaProducerService kafkaProducerService, ILogger<OrderProvider> logger)
        {
            _kafkaProducerService = kafkaProducerService ?? throw new ArgumentNullException(nameof(kafkaProducerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OrderResponse> CreatePayPalOrderAsync(CreateOrderRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ValidateOrderRequest(request);

            // Create Kafka message for payment processing
            var kafkaMessage = new PaymentKafkaMessage
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
                Currency = request.Currency,
                PayerEmail = request.PayerEmail,
                Description = request.Description,
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow
            };

            // Publish to Kafka for asynchronous processing
            await _kafkaProducerService.PublishPaymentMessageAsync(kafkaMessage).ConfigureAwait(false);

            _logger.LogInformation(
                "PayPal order created and published to Kafka. OrderId: {OrderId}, Amount: {Amount} {Currency}",
                request.OrderId,
                request.Amount,
                request.Currency);

            // Return response indicating order is being processed
            return new OrderResponse
            {
                OrderId = request.OrderId,
                Status = "PROCESSING",
                Amount = request.Amount,
                Currency = request.Currency,
                PayerEmail = request.PayerEmail,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            };
        }

        private static void ValidateOrderRequest(CreateOrderRequest request)
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