using System;
using NServiceBus;

namespace Messages
{
    public class OrderProcessed : IEvent
    {
        public string OrderId { get; set; }
        public string ProcessedBy { get; set; } // "Dapr" to indicate it came from Dapr
        public DateTime ProcessedAt { get; set; }
    }
}