#nullable enable
using System;

namespace StockApi.Models
{
    public class StockPriceResponse
    {
        public string Ticker { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public decimal ClosePrice { get; set; }
    }
}