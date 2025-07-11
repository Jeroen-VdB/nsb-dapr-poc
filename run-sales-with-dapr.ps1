#!/usr/bin/env pwsh

# Run the NServiceBus Sales service with Dapr sidecar
# This enables the Sales service to publish to Dapr pub/sub

Write-Host "Starting NServiceBus Sales service with Dapr sidecar..." -ForegroundColor Green
Write-Host "This will enable Sales to publish messages to both NServiceBus and Dapr pub/sub" -ForegroundColor Yellow
Write-Host ""

# Check if Dapr CLI is installed
try {
    $null = Get-Command dapr -ErrorAction Stop
}
catch {
    Write-Host "Error: Dapr CLI is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install Dapr CLI from https://docs.dapr.io/getting-started/install-dapr-cli/" -ForegroundColor Yellow
    exit 1
}

# Run with Dapr CLI
# --app-id: unique identifier for this service in Dapr
# --resources-path: path to Dapr components (uses the same as other Dapr services)
# --log-level: set to info for debugging
$daprArgs = @(
    "run"
    "--app-id", "sales-nsb"
    "--resources-path", "./Dapr/components/"
    "--log-level", "info"
    "--"
    "dotnet", "run", "--project", "NServiceBus/Sales/Sales.csproj"
)

Write-Host "Executing: dapr $($daprArgs -join ' ')" -ForegroundColor Cyan
Write-Host ""

& dapr $daprArgs

# Check exit code
if ($LASTEXITCODE -ne 0) {
    Write-Host "Dapr run failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}