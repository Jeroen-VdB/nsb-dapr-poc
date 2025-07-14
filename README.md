# nsb-dapr-poc
Proof of concept to validate bidirectional messaging between a service using NServiceBus and a service using Dapr.

# Prerequisites
1. VS Code
1. dotnet 8
1. Docker Desktop or Podman
1. Dapr
1. Azure Service Bus Namespace Standard or Premium

# Running the Demo

## Receiver (NServiceBus)
```ps1
cd ./Receiver
$Env:AzureServiceBus_ConnectionString="Endpoint=sb://..."
dotnet run 
```

## Sender (Dapr)
Set the AzureServiceBus ConnectionString in `./components/servicebus-pubsub.yaml`

```ps1
cd ./Sender
dapr run --app-id sender --components-path ../components -- dotnet run
```

The Dapr configuration uses `rawPayload: true` to disable CloudEvents wrapping. The metadata includes:
- NServiceBus.EnclosedMessageTypes header for compatibility
- Message ID

# References
- [Azure Service Bus transport native integration sample
](https://docs.particular.net/samples/azure-service-bus-netstandard/native-integration/)
- [Dapr Pub/Sub building block](https://docs.dapr.io/developing-applications/building-blocks/pubsub/)
