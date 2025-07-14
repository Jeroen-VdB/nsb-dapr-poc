using System;
using System.Collections.Generic;
using Dapr.Client;

Console.Title = "Sender";

// Dapr pubsub component name - this should match your Dapr component configuration
const string pubSubName = "servicebus-pubsub";
const string topicName = "Samples.ASB.NativeIntegration";

// Create DaprClient
var daprClient = new DaprClientBuilder().Build();

#region SerializedMessage

var nativeMessage = new NativeMessage
{
    Content = "Hello from Dapr sender",
    SentOnUtc = DateTime.UtcNow
};

#endregion

// Create metadata for NServiceBus compatibility
var metadata = new Dictionary<string, string>
{
    #region NecessaryHeaders
    ["NServiceBus.EnclosedMessageTypes"] = typeof(NativeMessage).FullName,
    #endregion
    ["NServiceBus.MessageId"] = Guid.NewGuid().ToString(),
    ["rawPayload"] = "true" // Indicate that this is a raw payload message
};

// Publish message using Dapr with raw payload (CloudEvents disabled)
await daprClient.PublishEventAsync(pubSubName, topicName, nativeMessage, metadata);

Console.WriteLine($"Native message sent via Dapr on {nativeMessage.SentOnUtc} UTC");
Console.WriteLine("Raw payload mode enabled (CloudEvents disabled)");