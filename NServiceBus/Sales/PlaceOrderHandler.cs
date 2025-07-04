using System;
using System.Threading.Tasks;
using Messages;
using Microsoft.Extensions.Logging;
using NServiceBus;
using Dapr.Client;
using System.Text.Json.Serialization;

namespace Sales
{    
    // Dapr-compatible order format
    public record DaprOrder([property: JsonPropertyName("orderId")] string OrderId);
    
    public class PlaceOrderHandler :
        IHandleMessages<PlaceOrder>
    {
        static readonly Random random = new Random();
        private readonly ILogger<PlaceOrderHandler> logger;
        private readonly DaprClient daprClient;

        public PlaceOrderHandler(ILogger<PlaceOrderHandler> logger, DaprClient daprClient)
        {
            this.logger = logger;
            this.daprClient = daprClient;
        }        

         public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            logger.LogInformation("Received PlaceOrder, OrderId = {orderId}",  message.OrderId);

            // This is normally where some business logic would occur

            #region ThrowTransientException
            // Uncomment to test throwing transient exceptions
            //if (random.Next(0, 5) == 0)
            //{
            //    throw new Exception("Oops");
            //}
            #endregion

            #region ThrowFatalException
            // Uncomment to test throwing fatal exceptions
            //throw new Exception("BOOM");
            #endregion

            var orderPlaced = new OrderPlaced
            {
                OrderId = message.OrderId
            };

            logger.LogInformation("Publishing OrderPlaced, OrderId = {orderId}", message.OrderId);

            // Publish to NServiceBus
            await context.Publish(orderPlaced);

            // Also publish to Dapr pub/sub
            var daprOrder = new DaprOrder(message.OrderId);
            await daprClient.PublishEventAsync("orderpubsub", "orders", daprOrder, context.CancellationToken);
            logger.LogInformation("Published order to Dapr pub/sub, OrderId = {orderId}", message.OrderId);
        }
    }
}
