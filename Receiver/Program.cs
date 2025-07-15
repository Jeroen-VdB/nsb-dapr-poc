using System;
using Microsoft.Extensions.Hosting;
using NServiceBus;

Console.Title = "NativeIntegration";

var builder = Host.CreateApplicationBuilder(args);

var endpointConfiguration = new EndpointConfiguration("Samples.ASB.NativeIntegration");

endpointConfiguration.EnableInstallers();
endpointConfiguration.UseSerialization<CloudEventsSerializerDefinition>();
endpointConfiguration.Conventions().DefiningMessagesAs(type => type.Name == "NativeMessage");

var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception("Could not read the 'AzureServiceBus_ConnectionString' environment variable. Check the sample prerequisites.");
}

var transport = new AzureServiceBusTransport(connectionString, TopicTopology.Default);
endpointConfiguration.UseTransport(transport);

Console.WriteLine("Starting...");

builder.UseNServiceBus(endpointConfiguration);
await builder.Build().RunAsync();
