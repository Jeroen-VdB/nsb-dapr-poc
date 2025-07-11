using System.Text.Json.Serialization;
using Dapr;
using NServiceBus;
using Messages;

var builder = WebApplication.CreateBuilder(args);

// Configure NServiceBus as a send-only endpoint
var endpointConfiguration = new EndpointConfiguration("order-processor");
endpointConfiguration.SendOnly();
endpointConfiguration.UseSerialization<SystemJsonSerializer>();
endpointConfiguration.UseTransport<LearningTransport>();

// Route the OrderProcessed event to the Billing endpoint
var routing = endpointConfiguration.UseTransport<LearningTransport>().Routing();
routing.RouteToEndpoint(typeof(OrderProcessed), "Billing");

builder.UseNServiceBus(endpointConfiguration);

var app = builder.Build();

// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// needed for Dapr pub/sub routing
app.MapSubscribeHandler();

if (app.Environment.IsDevelopment()) {app.UseDeveloperExceptionPage();}

// Dapr subscription in [Topic] routes orders topic to this route
app.MapPost("/orders", [Topic("orderpubsub", "orders")] async (Order order, IMessageSession messageSession) => {
    Console.WriteLine("Subscriber received : " + order);
    
    // Send OrderProcessed event to NServiceBus
    var orderProcessed = new OrderProcessed
    {
        OrderId = order.OrderId,
        ProcessedBy = "Dapr",
        ProcessedAt = DateTime.UtcNow
    };
    
    await messageSession.Send(orderProcessed);
    Console.WriteLine($"Sent OrderProcessed event to NServiceBus for OrderId: {order.OrderId}");
    
    return Results.Ok(order);
});

await app.RunAsync();

public record Order([property: JsonPropertyName("orderId")] string OrderId);
