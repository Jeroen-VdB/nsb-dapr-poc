# nsb-dapr-poc
Proof of concept to validate bidirectional messaging between a service using NServiceBus and a service using Dapr.

# Prerequisites
1. VS or VS Code
1. dotnet 8 & 9
1. Docker Desktop or Podman
1. https://docs.dapr.io/getting-started/install-dapr-cli/
1. https://docs.dapr.io/getting-started/install-dapr-selfhost/
   - `dapr init`

# Run the demo


## NServiceBus initial test
🐞 In VS Code "Run and Debug": `All NServiceBus Projects`


## Dapr initial test
🐞 Execute command `dapr run -f ./Dapr`
