# nsb-dapr-poc
Proof of concept to validate bidirectional messaging between a service using NServiceBus and a service using Dapr.

# Prerequisites
1. .NET 8
1. Docker Desktop or Podman
1. Dapr CLI
1. Azure Service Bus Namespace Standard or Premium

# Running the Demo

## Sender (Dapr)
First, set the AzureServiceBus ConnectionString in `./components/servicebus-pubsub.yaml`.

Next, run the sender:

```ps1
cd ./Sender
dapr run --app-id sender --components-path ../components -- dotnet run
```

- The Dapr configuration uses `rawPayload: true` to disable CloudEvents wrapping.
- The publisher uses PascalCasing via JsonSerializerOptions to be NServiceBus compatible.
- The metadata includes NServiceBus.EnclosedMessageTypes header for NServiceBus compatibility.

## Receiver (NServiceBus)
Run the receiver:
```ps1
cd ./Receiver
$Env:AzureServiceBus_ConnectionString="Endpoint=sb://..."
dotnet run 
```

See the Dapr message coming in.

- The NServiceBus configuration uses SystemJsonSerializer to enable JSON deserialization.
- A convention is defined that types with the name `NativeMessage` and should be treated as messages, even if they don't implement NServiceBus interfaces.

# References
- [Azure Service Bus transport native integration sample
](https://docs.particular.net/samples/azure-service-bus-netstandard/native-integration/)
- [Dapr Pub/Sub building block](https://docs.dapr.io/developing-applications/building-blocks/pubsub/)
