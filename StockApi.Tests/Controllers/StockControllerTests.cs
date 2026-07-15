#nullable enable    
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using StockApi.Controllers;
using StockApi.Models;
using StockApi.Providers;

namespace StockApi.Tests.Controllers
{
    public class StockControllerTests
    {
        [Fact]
        public async Task GetPrice_MissingTicker_ReturnsBadRequest()
        {
            var providerMock = new Mock<IStockProvider>();
            var controller = new StockController(providerMock.Object);

            var result = await controller.GetPrice(null, null, CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Ticker is required", badRequest.Value!.ToString()!, StringComparison.OrdinalIgnoreCase);
            providerMock.Verify(p => p.GetStockPriceAsync(It.IsAny<StockPriceRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetPrice_ValidTicker_ReturnsOk()
        {
            var expected = new StockPriceResponse { Ticker = "AAPL", Date = new DateTime(2026,6,18), ClosePrice = 248.16M };
            var providerMock = new Mock<IStockProvider>();
            providerMock.Setup(p => p.GetStockPriceAsync(It.IsAny<StockPriceRequest>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expected);

            var controller = new StockController(providerMock.Object);

            var result = await controller.GetPrice("AAPL", new DateTime(2026,6,18), CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<StockPriceResponse>(ok.Value);
            Assert.Equal(expected.Ticker, value.Ticker);
            Assert.Equal(expected.ClosePrice, value.ClosePrice);
        }
    }
}