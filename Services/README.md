# Blue Explorer - infrastructure resource explorer (Azure-first)

Blue Explorer is an Azure-first infrastructure explorer built with .NET 8 (backend) and a separate web frontend.  
This project started as a fork of the original [PurpleExplorer](https://github.com/telstrapurple/PurpleExplorer) by Telstra Purple
and is now evolving independently as BlueExplorer.

It's a simple tool to help you:

* Connect to Azure Service Bus
* View topics and subscriptions
* View queues
* View active and dead-letter messages
* View message body and its details
* Send a new message
* Save messages to send them quickly
* Delete a message **^**
* Purge active or dead-letter messages
* Re-submit a message from dead-letter
* Dead-letter a message **^**

**\^ NOTE:** These marked actions require receiving all the messages up to the selected message and this increases DeliveryCount. Be aware that there can be consequences to other messages in the subscription.

## How to run (Service Bus API)

This project is provided only as source code.
You need to have [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or later installed.

To build and run the project:

1. Clone the repository.
2. Navigate to the `Services/` folder.
3. Run `dotnet run --project AzureServiceBus/ServiceBus.Api/ServiceBus.Api.csproj`.

1. Configure `AzureServiceBus/ServiceBus.Api/appsettings.json` with your Service Bus connections (or Key Vault).
2. Open `http://localhost:5209` (or the HTTPS URL shown in the console).

### Docker

Build and run (from the `Services/` folder):

1. `docker build -t blueexplorer-servicebus-api .`
2. `docker run -p 8080:8080 blueexplorer-servicebus-api`

Or use compose:

1. `docker compose up --build`

## Legal notice (Azure / Microsoft trademarks)

This project is not affiliated with, endorsed by, or sponsored by Microsoft.  
Microsoft, Azure, and related marks are trademarks of Microsoft Corporation.

## Credits

BlueExplorer is inspired by the original PurpleExplorer project by Telstra Purple.

## License

Licensed under the MIT License. See `LICENSE`.

## Recent Changes

Since forking from the original project, the following significant updates have been made:

* Enhanced UI:
  * resizable panels and grids;
  * filtering for the tree view;
  * improved message details window with DLQ reason display and application properties;
  * added spinner to indicate background operations.
* Improved connection management and user experience (taller connection box, alerts for existing connections, etc.).
* Upgraded to .NET 8 for better performance and latest features.
* Upgraded to latest Avalonia (v11) for improved UI framework capabilities.
* Migrated to Azure.Messaging.ServiceBus for the latest Azure SDK.
* Upgraded all dependencies to their latest versions.
* Implemented comprehensive nullability fixes and code quality improvements
* (editorconfig, better naming).
* Added key bindings and buttons for closing windows.
* Fixed various bugs including null reference exceptions, timeout issues, and app state handling.

For a full list of changes, see the git commit history.
