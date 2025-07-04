#!/bin/bash

# Run the NServiceBus Sales service with Dapr sidecar
# This enables the Sales service to publish to Dapr pub/sub

echo "Starting NServiceBus Sales service with Dapr sidecar..."
echo "This will enable Sales to publish messages to both NServiceBus and Dapr pub/sub"
echo ""

# Run with Dapr CLI
# --app-id: unique identifier for this service in Dapr
# --resources-path: path to Dapr components (uses the same as other Dapr services)
# --log-level: set to info for debugging
dapr run \
    --app-id sales-nsb \
    --resources-path ./Dapr/components/ \
    --log-level info \
    -- dotnet run --project NServiceBus/Sales/Sales.csproj