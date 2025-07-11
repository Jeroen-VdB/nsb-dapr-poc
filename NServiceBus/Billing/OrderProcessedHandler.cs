using System.Threading.Tasks;
using Messages;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Billing
{
    public class OrderProcessedHandler : IHandleMessages<OrderProcessed>
    {
        private readonly ILogger<OrderProcessedHandler> logger;

        public OrderProcessedHandler(ILogger<OrderProcessedHandler> logger)
        {
            this.logger = logger;
        }

        public Task Handle(OrderProcessed message, IMessageHandlerContext context)
        {
            logger.LogInformation("Received OrderProcessed from {processedBy}, OrderId = {orderId}, ProcessedAt = {processedAt}", 
                message.ProcessedBy, 
                message.OrderId, 
                message.ProcessedAt);
            
            // Here we could handle billing for orders that were processed by Dapr services
            logger.LogInformation("Successfully demonstrated Dapr → NServiceBus message flow!");
            
            return Task.CompletedTask;
        }
    }
}