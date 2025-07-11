# nsb-dapr-poc
Proof of concept to validate bidirectional messaging between a service using NServiceBus and a service using Dapr.

# Prerequisites
1. VS or VS Code
1. dotnet 8 & 9
1. Docker Desktop or Podman
1. https://docs.dapr.io/getting-started/install-dapr-cli/
1. https://docs.dapr.io/getting-started/install-dapr-selfhost/
    - `dapr init`

## NServiceBus initial test
🐞 In VS Code "Run and Debug": `All NServiceBus Projects`

## Dapr initial test
🐞 Execute command `dapr run -f ./Dapr`

# Bidirectional NServiceBus ↔ Dapr Integration Plan
     
## Overview     
 Create a proof of concept demonstrating bidirectional messaging between 
  NServiceBus and Dapr services by implementing bridge components and    
 extending existing services.     
    
 ### Architecture Design
    
 Option 1: Direct Integration (Recommended for POC)    
    
 - Modify NServiceBus services to also use Dapr SDK    
 - Modify Dapr services to also use NServiceBus client    
 - Each service maintains dual messaging capabilities     
    
 Option 2: Bridge Service Pattern    
    
 - Create dedicated bridge service(s) that translate between NServiceBus 
  and Dapr   
 - Maintains separation of concerns but adds complexity   
    
 ### Implementation Steps    
    
 Phase 1: NServiceBus → Dapr Communication    
    
 1. Extend Sales Service    
    - Add Dapr.AspNetCore NuGet package     
    - Configure Dapr client in Program.cs   
    - Modify PlaceOrderHandler to also publish to Dapr after publishing   
 OrderPlaced event    
    - Publish to "orders" topic on "orderpubsub" component    
 2. Verify Dapr Reception   
    - Ensure order-processor receives messages from NServiceBus Sales     
 service     
    - Log correlation between NServiceBus OrderId and Dapr message     
    
 Phase 2: Dapr → NServiceBus Communication    
    
 1. Create NServiceBus Endpoint in order-processor     
    - Add NServiceBus NuGet package to order-processor     
    - Configure as send-only endpoint    
    - Send OrderProcessed command to Billing service when order is     
 received    
 2. Extend Billing Service     
    - Create OrderProcessedHandler for new OrderProcessed command   
    - Add routing configuration for OrderProcessed messages   
    
 Phase 3: Message Translation     
    
 1. Create Shared Contracts    
    - Define common message formats that both systems understand    
    - Implement serialization/deserialization logic     
 2. Add Message Mapping     
    - Map NServiceBus ICommand/IEvent to Dapr JSON payloads   
    - Handle CloudEvents envelope format from Dapr   
    
 Phase 4: Validation & Testing    
    
 1. End-to-End Flow Testing    
    - ClientUI → Sales (NSB) → order-processor (Dapr) → Billing (NSB)     
    - checkout (Dapr) → Sales (NSB) → Billing (NSB)     
 2. Error Handling    
    - Test message failures and retries across both systems   
    - Ensure transactional consistency   
    
 Technical Considerations   
    
 - Message Formats: Need to handle NServiceBus binary vs Dapr JSON    
 - Topic/Queue Mapping: Map NServiceBus endpoints to Dapr topics   
 - Error Handling: Different retry mechanisms between systems   
 - Monitoring: Unified logging and tracing across both platforms   
    
 Success Criteria     
    
 - Messages flow from NServiceBus to Dapr services     
 - Messages flow from Dapr to NServiceBus services     
 - Both systems maintain their native error handling   
 - Minimal code changes to existing services 

 # NServiceBus ↔ Dapr Integration Guide

This proof of concept demonstrates bidirectional messaging between NServiceBus and Dapr services.

## What We've Implemented

### Phase 1: NServiceBus → Dapr Communication ✅
- Modified the NServiceBus `Sales` service to publish messages to both NServiceBus and Dapr
- When an order is placed through the NServiceBus ClientUI:
  1. Sales service receives the `PlaceOrder` command
  2. Sales publishes `OrderPlaced` event to NServiceBus (received by Billing)
  3. Sales ALSO publishes the order to Dapr pub/sub (received by order-processor)

### Phase 2: Dapr → NServiceBus Communication ✅
- Modified the Dapr `order-processor` to send messages to NServiceBus
- When order-processor receives an order from Dapr pub/sub:
  1. It processes the order
  2. It sends an `OrderProcessed` event to NServiceBus
  3. The Billing service receives and handles this event

## Architecture Overview

```
┌─────────────┐     ┌─────────────┐      ┌─────────────┐
│  ClientUI   │────▶│    Sales    │────▶│   Billing   │
│   (NSB)     │     │    (NSB)    │      │    (NSB)    │
└─────────────┘     └──────┬──────┘      └──────▲──────┘
                           │                    │
                           │ Publishes to       │ Receives
                           │ Dapr pub/sub       │ OrderProcessed
                           ▼                    │
                    ┌─────────────┐      ┌──────┴──────┐
                    │    Redis    │      │   order-    │
                    │  (pub/sub)  │────▶│  processor  │
                    └─────────────┘      │   (Dapr)    │
                                         └─────────────┘
```

## Running the Integration

### Prerequisites
- .NET 9.0 SDK
- Dapr CLI installed (`dapr init`)
- Redis running (installed by Dapr init)

### Option 1: Run Everything Together

1. Start Redis (if not already running via Dapr):
   ```bash
   dapr init
   ```

2. Start all Dapr services:
   ```bash
   dapr run -f ./Dapr
   ```

3. In separate terminals, start NServiceBus services with Dapr sidecars:

   Windows:
   
   ```ps1
   # Terminal 1 - Sales with Dapr
   ./run-sales-with-dapr.ps1
   
   # Terminal 2 - Billing (regular NServiceBus)
   dotnet run --project NServiceBus/Billing/Billing.csproj
   
   # Terminal 3 - ClientUI (regular NServiceBus)  
   dotnet run --project NServiceBus/ClientUI/ClientUI.csproj
   ```

   Unix:
   ```bash
   # Terminal 1 - Sales with Dapr
   ./run-sales-with-dapr.sh
   
   # Terminal 2 - Billing (regular NServiceBus)
   dotnet run --project NServiceBus/Billing/Billing.csproj
   
   # Terminal 3 - ClientUI (regular NServiceBus)  
   dotnet run --project NServiceBus/ClientUI/ClientUI.csproj
   ```

4. Test the integration:
   - Open http://localhost:5105 (ClientUI)
   - Place an order
   - Watch the logs to see messages flow through both systems

### Option 2: Use VS Code Debugging

1. The launch.json has been configured for debugging
2. Use the "All NServiceBus Projects" compound configuration
3. Run Dapr services separately: `dapr run -f ./Dapr`

## Message Flow Examples

### Example 1: NServiceBus → Dapr
```
1. User places order in ClientUI (OrderId: "123")
2. Sales receives PlaceOrder command
3. Sales publishes to NServiceBus: OrderPlaced { OrderId: "123" }
4. Sales publishes to Dapr: { "orderId": "123" }
5. Billing receives OrderPlaced from NServiceBus
6. order-processor receives order from Dapr pub/sub
```

### Example 2: Dapr → NServiceBus
```
1. checkout publishes order to Dapr pub/sub
2. order-processor receives order from Redis
3. order-processor sends OrderProcessed to NServiceBus
4. Billing receives OrderProcessed event
```

## Technical Implementation Details

### NServiceBus Sales Service Modifications
- Added Dapr.Client NuGet package
- Configured DaprClient in Program.cs
- Modified PlaceOrderHandler to publish to both systems

### Dapr order-processor Modifications
- Added NServiceBus NuGet packages
- Configured as NServiceBus send-only endpoint
- Sends OrderProcessed events when orders are received

### Message Format Translation
- NServiceBus uses strongly-typed classes (ICommand/IEvent)
- Dapr uses JSON payloads
- Translation happens in the handlers

## Next Steps for Production

1. **Error Handling**: Implement proper error handling for cross-system failures
2. **Transaction Support**: Consider distributed transaction patterns
3. **Message Deduplication**: Prevent duplicate processing across systems
4. **Monitoring**: Unified logging and tracing across both platforms
5. <span style="color:orange">**Bridge Service**: Consider a dedicated bridge service for complex scenarios</span>
6. **Performance**: Optimize for high-throughput scenarios

## Troubleshooting

### Common Issues

1. **"No connection could be made" errors**
   - Ensure Redis is running: `docker ps | grep redis`
   - Restart Dapr: `dapr init`

2. **Messages not flowing to Dapr**
   - Ensure Sales is running with Dapr sidecar
   - Check Dapr components are loaded: `dapr components list`

3. **NServiceBus transport errors**
   - Check the `.learningtransport` folder exists
   - Ensure all services are using the same transport configuration

## Summary

This POC successfully demonstrates that:
- ✅ NServiceBus services can publish to Dapr pub/sub
- ✅ Dapr services can send messages to NServiceBus
- ✅ Both systems can coexist and interoperate
- ✅ Organizations can gradually migrate between the systems