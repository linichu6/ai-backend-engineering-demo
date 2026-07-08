#nullable enable
using System;

namespace OrderApi.Models
{
    public class CreateOrderRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string PayerEmail { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}