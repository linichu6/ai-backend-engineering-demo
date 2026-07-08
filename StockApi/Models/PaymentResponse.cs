#nullable enable
using System;

namespace StockApi.Models
{
    public class PaymentResponse
    {
        public string OrderId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string PayerEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}