#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
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

            var provider = new StockProvider(repoMock.Object);
            var request = new StockPriceRequest { Ticker = ticker, Date = date };

            // Act
            var result = await provider.GetStockPriceAsync(request);

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
            var provider = new StockProvider(repoMock.Object);
            var request = new StockPriceRequest { Ticker = " " };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => provider.GetStockPriceAsync(request));
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

            var provider = new StockProvider(repoMock.Object);
            var request = new StockPriceRequest { Ticker = ticker, Date = date };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => provider.GetStockPriceAsync(request));
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

            var provider = new StockProvider(repoMock.Object);
            var request = new StockPriceRequest { Ticker = ticker, Date = null };

            // Act
            var result = await provider.GetStockPriceAsync(request);

            // Assert
            Assert.NotNull(capturedDate);
            Assert.Equal(DateTime.UtcNow.Date, capturedDate!.Value.Date);
            repoMock.Verify(r => r.GetStockPriceAsync(ticker, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}