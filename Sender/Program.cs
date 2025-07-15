using System;
using System.Collections.Generic;
using System.Text.Json;
using Dapr.Client;

Console.Title = "Sender";

// Dapr pubsub component name - this should match your Dapr component configuration
const string pubSubName = "servicebus-pubsub";
const string topicName = "Samples.ASB.NativeIntegration";

// Create DaprClient with PascalCase JSON serialization
var daprClient = new DaprClientBuilder()
    .Build();
    

var nativeMessage = new NativeMessage
{
    Content = "Hello from Dapr sender",
    SentOnUtc = DateTime.UtcNow
};

// Create metadata for NServiceBus compatibility
var metadata = new Dictionary<string, string>
{
    #region NecessaryHeaders
    ["NServiceBus.EnclosedMessageTypes"] = typeof(NativeMessage).FullName,
    #endregion
    ["NServiceBus.MessageId"] = Guid.NewGuid().ToString(),
};

await daprClient.PublishEventAsync(pubSubName, topicName, nativeMessage, metadata);

Console.WriteLine($"Native message sent via Dapr on {nativeMessage.SentOnUtc} UTC");
Console.WriteLine("Raw payload mode enabled (CloudEvents disabled)");