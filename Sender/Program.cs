using System;
using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

Console.Title = "Sender";

var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception("Could not read the 'AzureServiceBus_ConnectionString' environment variable. Check the sample prerequisites.");
}

// Create a Service Bus Administration client to manage queues
var adminClient = new ServiceBusAdministrationClient(connectionString);
const string queueName = "Samples.ASB.NativeIntegration";

// Check if queue exists and create it if it doesn't
if (!await adminClient.QueueExistsAsync(queueName))
{
    Console.WriteLine($"Creating queue: {queueName}");    
    await adminClient.CreateQueueAsync(queueName);
    Console.WriteLine($"Queue {queueName} created successfully");
}
else
{
    Console.WriteLine($"Queue {queueName} exists");
}

var serviceBusClient = new ServiceBusClient(connectionString);
var serviceBusSender = serviceBusClient.CreateSender(queueName);

#region SerializedMessage

var nativeMessage = new NativeMessage
{
    Content = "Hello from native sender",
    SentOnUtc = DateTime.UtcNow
};

var json = JsonSerializer.Serialize(nativeMessage);

#endregion

var bytes = Encoding.UTF8.GetBytes(json);

var message = new ServiceBusMessage(bytes)
{
    MessageId = Guid.NewGuid().ToString(),

    ApplicationProperties =
    {
        #region NecessaryHeaders
        ["NServiceBus.EnclosedMessageTypes"] = typeof(NativeMessage).FullName
        #endregion
    }
};

await serviceBusSender.SendMessageAsync(message);

Console.WriteLine($"Native message sent on {nativeMessage.SentOnUtc} UTC");
Console.WriteLine("Press any key to exit");
Console.ReadKey();