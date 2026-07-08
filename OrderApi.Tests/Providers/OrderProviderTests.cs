#nullable enable
using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using OrderApi.Models;
using OrderApi.Providers;
using OrderApi.Services;
using Microsoft.Extensions.Logging;

namespace OrderApi.Tests.Providers
{
    public class OrderProviderTests
    {
        [Fact]
        public async Task CreatePayPalOrderAsync_WithValidRequest_ReturnsOrderResponse()
        {
            // Arrange
            var mockKafkaService = new Mock<IKafkaProducerService>();
            var mockLogger = new Mock<ILogger<OrderProvider>>();
            var provider = new OrderProvider(mockKafkaService.Object, mockLogger.Object);

            var request = new CreateOrderRequest
            {
                OrderId = "ORD-12345",
                Amount = 99.99m,
                Currency = "USD",
                PayerEmail = "customer@example.com",
                Description = "Test order"
            };

            // Act
            var response = await provider.CreatePayPalOrderAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("ORD-12345", response.OrderId);
            Assert.Equal(99.99m, response.Amount);
            Assert.Equal("USD", response.Currency);
            Assert.Equal("PROCESSING", response.Status);
            mockKafkaService.Verify(k => k.PublishPaymentMessageAsync(It.IsAny<PaymentKafkaMessage>()), Times.Once);
        }

        [Fact]
        public async Task CreatePayPalOrderAsync_WithMissingOrderId_ThrowsArgumentException()
        {
            // Arrange
            var mockKafkaService = new Mock<IKafkaProducerService>();
            var mockLogger = new Mock<ILogger<OrderProvider>>();
            var provider = new OrderProvider(mockKafkaService.Object, mockLogger.Object);

            var request = new CreateOrderRequest
            {
                OrderId = string.Empty,
                Amount = 99.99m,
                Currency = "USD",
                PayerEmail = "customer@example.com",
                Description = "Test order"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => provider.CreatePayPalOrderAsync(request));
            Assert.Contains("OrderId is required", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        public async Task CreatePayPalOrderAsync_WithInvalidAmount_ThrowsArgumentException(decimal amount)
        {
            // Arrange
            var mockKafkaService = new Mock<IKafkaProducerService>();
            var mockLogger = new Mock<ILogger<OrderProvider>>();
            var provider = new OrderProvider(mockKafkaService.Object, mockLogger.Object);

            var request = new CreateOrderRequest
            {
                OrderId = "ORD-12345",
                Amount = amount,
                Currency = "USD",
                PayerEmail = "customer@example.com",
                Description = "Test order"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => provider.CreatePayPalOrderAsync(request));
            Assert.Contains("Amount must be greater than 0", ex.Message);
        }
    }
}