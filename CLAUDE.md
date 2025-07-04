# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

This is a proof-of-concept project demonstrating bidirectional messaging between NServiceBus and Dapr services. The project contains parallel implementations of an order processing system using both messaging frameworks.

**Goal Architecture**: Enable direct communication between NServiceBus and Dapr services, allowing messages to flow bidirectionally across both messaging systems. This would allow organizations to gradually migrate from NServiceBus to Dapr or run hybrid architectures where both systems coexist and interoperate.

## Build Commands

```bash
# Build entire solution
dotnet build NsbDaprPoc.slnx

# Build NServiceBus solution only
dotnet build NServiceBus/RetailDemo.sln

# Build individual projects
dotnet build NServiceBus/ClientUI/ClientUI.csproj
dotnet build NServiceBus/Sales/Sales.csproj
dotnet build NServiceBus/Billing/Billing.csproj
dotnet build Dapr/checkout/checkout.csproj
dotnet build Dapr/order-processor/order-processor.csproj
```

## Run Commands

### NServiceBus Services
```bash
# Using VS Code (recommended)
# Use the "All NServiceBus Projects" compound configuration in the debugger

# Or run individually
dotnet run --project NServiceBus/ClientUI/ClientUI.csproj
dotnet run --project NServiceBus/Sales/Sales.csproj
dotnet run --project NServiceBus/Billing/Billing.csproj
```

### Dapr Services
```bash
# With Dapr CLI (requires dapr init)
dapr run -f ./Dapr

# Or run individually
dapr run --app-id order-processor-sdk --app-port 7006 -- dotnet run --project Dapr/order-processor/order-processor.csproj
dapr run --app-id checkout-sdk -- dotnet run --project Dapr/checkout/checkout.csproj
```

## Architecture

### NServiceBus Implementation
- **ClientUI** (http://localhost:5105): ASP.NET Core web app for placing orders
- **Sales**: Handles PlaceOrder commands, publishes OrderPlaced events
- **Billing**: Subscribes to OrderPlaced events
- **Messages**: Shared message contracts library

Message flow: ClientUI → PlaceOrder command → Sales → OrderPlaced event → Billing

### Dapr Implementation
- **checkout**: Console app that publishes orders to Dapr pub/sub
- **order-processor** (http://localhost:7006/swagger): ASP.NET Core API subscribing to orders

Message flow: checkout → publishes to "orders" topic (Redis) → order-processor

### Key Architectural Differences
1. **Message Contracts**: NServiceBus uses strongly-typed ICommand/IEvent interfaces; Dapr uses JSON payloads
2. **Message Handling**: NServiceBus uses message handlers; Dapr uses HTTP endpoints with topic subscriptions
3. **Infrastructure**: NServiceBus configures transport directly; Dapr abstracts via components (Redis for pub/sub)

### Integration Points (Goal)
The POC aims to establish bidirectional communication patterns:
- **NServiceBus → Dapr**: NServiceBus services publishing events that Dapr services can subscribe to
- **Dapr → NServiceBus**: Dapr services publishing events that NServiceBus handlers can process
- **Bridge Component**: Potential adapter service to translate between NServiceBus message formats and Dapr pub/sub topics

## Development Notes

- **Frameworks**: .NET 8.0 (Dapr), .NET 9.0 (NServiceBus/PlatformLauncher)
- **No test projects currently exist**
- **Configuration**: Standard ASP.NET Core pattern with appsettings.json and appsettings.Development.json
- **PlatformLauncher**: Currently disabled (commented out) - would enable Particular Service Platform tools

## Common Development Tasks

### Debugging
- Use VS Code's compound debug configuration "All NServiceBus Projects" to run all NServiceBus services together
- Individual debug configurations available for each service

### Adding New Messages (NServiceBus)
1. Add message class to Messages project implementing ICommand or IEvent
2. Create handler in appropriate service inheriting from IHandleMessages<T>
3. Configure routing if needed

### Adding New Subscriptions (Dapr)
1. Add subscription endpoint in order-processor with [Topic] attribute
2. Configure topic subscription in Dapr components