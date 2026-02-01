# Agent Guide for BlueExplorer

This document provides essential information for AI agents and developers working on the BlueExplorer project.

## Project Overview
BlueExplorer is an Azure-first infrastructure explorer. The backend exposes APIs per resource (Service Bus now; Redis/Blob later).

## Tech Stack
- **Framework:** .NET 8.0 (backend APIs)
- **Runtime:** .NET 8.0
- **Azure SDK:** `Azure.Messaging.ServiceBus`

## Project Structure
- `Services/`: Backend (.NET).
    - `AzureServiceBus/ServiceBus.Api`: Service Bus API.
    - `AzureServiceBus/ServiceBus.Application`: Service Bus use-case interfaces and services.
    - `AzureServiceBus/ServiceBus.Infrastructure`: Service Bus integrations (Azure SDK).
    - `AzureServiceBus/ServiceBus.Domain`: Service Bus domain models.
    - `AzureRedis/AZRedis.Api`: Azure Redis API (scaffold).
    - `AzureRedis/AZRedis.Application`: Azure Redis use-case interfaces and services (scaffold).
    - `AzureRedis/AZRedis.Infrastructure`: Azure Redis integrations (scaffold).
    - `AzureRedis/AZRedis.Domain`: Azure Redis domain models.
    - `Commons/`: Shared API response models and result types.
- `Apps/`: Frontend (Next.js to be added).

## Key Components & Responsibilities
### Presentation
- **ServiceBus.Api**: HTTP endpoints and connection management for Service Bus.

### Infrastructure
- **ServiceBus.Infrastructure**: Azure Service Bus SDK integrations.

### State Management
- Connection data is stored by the ServiceBus application layer (see `Services/AzureServiceBus/ServiceBus.Application/Services`).

## Important Notes for Agents
- **Destructive Actions**: Actions like "Delete Message" or "Purge" have significant consequences. Ensure the user is aware of the risks (as noted in the README regarding `DeliveryCount`).
- **Azure SDK**: The Service Bus API uses `Azure.Messaging.ServiceBus`. 
- **Tests**: Currently, the project lacks automated tests. When adding new features, consider adding unit tests in a separate test project (e.g., `BlueExplorer.Tests`).

## Development Workflow
- **Running the API:** From the `Services/` folder, run `dotnet run --project AzureServiceBus/ServiceBus.Api/ServiceBus.Api.csproj`.
- **Building:** `dotnet build`.
- **Formatting:** Follow the existing C# coding style (standard .NET conventions).
    - **Order of class members**: The order of class members should follow a logical sequence, starting with fields (constants, then readonly, then instance),
      then constructors, followed by properties, and finally methods.  
      - Within each category, the order is: public, protected, internal, private.
      - There should be one blank line between each category.
      - There should be no public fields.
      - This makes the code easier to read and understand.
