#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using StockApi.Models;
using StockApi.Providers;
using StockApi.Repositories;
using System.Collections.Generic;

namespace StockApi.Tests.Providers
{
    public class StockProviderTests
    {
        [Fact]
        public async Task GetStockPriceAsync_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var ticker = "MSFT";
            var date = new DateTime(2026, 6, 19);
            var expected = new StockPriceResponse { Ticker = ticker, Date = date, ClosePrice = 612.41M };

            var repoMock = new Mock<IStockRepository>();
            repoMock.Setup(r => r.GetStockPriceAsync(ticker, date, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expected);

            var loggerMock = new Mock<ILogger<StockProvider>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var provider = new StockProvider(repoMock.Object, memoryCache, loggerMock.Object);
            var request = new StockPriceRequest { Ticker = ticker, Date = date };

            // Act
            var result = await provider.GetStockPriceAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(expected.Ticker, result.Ticker);
            Assert.Equal(expected.ClosePrice, result.ClosePrice);
            repoMock.Verify(r => r.GetStockPriceAsync(ticker, date, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetStockPriceAsync_EmptyTicker_ThrowsArgumentException()
        {
            // Arrange
            var repoMock = new Mock<IStockRepository>();
            var loggerMock = new Mock<ILogger<StockProvider>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var provider = new StockProvider(repoMock.Object, memoryCache, loggerMock.Object);
            var request = new StockPriceRequest { Ticker = " " };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => provider.GetStockPriceAsync(request, CancellationToken.None));
            repoMock.Verify(r => r.GetStockPriceAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetStockPriceAsync_RepositoryReturnsNull_ThrowsKeyNotFoundException()
        {
            // Arrange
            var ticker = "INVALID";
            var date = new DateTime(2026, 6, 18);

            var repoMock = new Mock<IStockRepository>();
            repoMock.Setup(r => r.GetStockPriceAsync(ticker, date, It.IsAny<CancellationToken>())).ReturnsAsync((StockPriceResponse?)null);

            var loggerMock = new Mock<ILogger<StockProvider>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var provider = new StockProvider(repoMock.Object, memoryCache, loggerMock.Object);
            var request = new StockPriceRequest { Ticker = ticker, Date = date };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => provider.GetStockPriceAsync(request, CancellationToken.None));
            repoMock.Verify(r => r.GetStockPriceAsync(ticker, date, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetStockPriceAsync_NullDate_UsesUtcNowDate()
        {
            // Arrange
            var ticker = "AAPL";
            DateTime? capturedDate = null;

            var repoMock = new Mock<IStockRepository>();
            repoMock.Setup(r => r.GetStockPriceAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                    .Callback<string, DateTime, CancellationToken>((t, d, ct) => capturedDate = d)
                    .ReturnsAsync(new StockPriceResponse { Ticker = ticker, Date = DateTime.UtcNow.Date, ClosePrice = 1M });

            var loggerMock = new Mock<ILogger<StockProvider>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var provider = new StockProvider(repoMock.Object, memoryCache, loggerMock.Object);
            var request = new StockPriceRequest { Ticker = ticker, Date = null };

            // Act
            var result = await provider.GetStockPriceAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedDate);
            Assert.Equal(DateTime.UtcNow.Date, capturedDate!.Value.Date);
            repoMock.Verify(r => r.GetStockPriceAsync(ticker, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetStockPriceAsync_CacheHit_ReturnsCachedValueAndSkipsRepository()
        {
            // Arrange
            var ticker = "GOOG";
            var date = new DateTime(2026, 6, 20);
            var expected = new StockPriceResponse { Ticker = ticker, Date = date, ClosePrice = 1500.00M };

            var repoMock = new Mock<IStockRepository>();

            var loggerMock = new Mock<ILogger<StockProvider>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            // Pre-populate cache with the expected response using the same cache key format as provider
            string cacheKey = $"stock:{ticker}:{date:yyyy-MM-dd}";
            memoryCache.Set(cacheKey, expected);

            var provider = new StockProvider(repoMock.Object, memoryCache, loggerMock.Object);
            var request = new StockPriceRequest { Ticker = ticker, Date = date };

            // Act
            var result = await provider.GetStockPriceAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(expected.Ticker, result.Ticker);
            Assert.Equal(expected.ClosePrice, result.ClosePrice);
            repoMock.Verify(r => r.GetStockPriceAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}