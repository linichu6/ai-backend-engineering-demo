#nullable enable
using System;

namespace StockApi.Models
{
    public class StockPriceRequest
    {
        public string Ticker { get; set; } = string.Empty;

        public DateTime? Date { get; set; }
    }
}