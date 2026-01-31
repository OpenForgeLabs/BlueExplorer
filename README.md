# Purple Explorer - cross-platform Azure Service Bus explorer (Windows, macOS, Linux)

Purple Explorer is a cross-platform desktop application built with .NET 8.  
This repository is a fork of the original [PurpleExplorer](https://github.com/telstrapurple/PurpleExplorer) project,
which appears to be no longer maintained. This fork attempts to continue development, albeit at a slow pace.

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

## How to run

This project is provided only as source code.
You need to have [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or later installed.

To build and run the project:

1. Clone the repository.
2. Navigate to the root of the repo.
3. Run `dotnet run --project PurpleExplorer/PurpleExplorer.csproj` to run the application;
   or after building with `dotnet build`,
   run `dotnet run PurpleExplorer/bin/Debug/net8.0/PurpleExplorer.dll`.

## Web version (API + Blazor WASM)

The web version is self-hosted by `PurpleExplorer.Api` and serves the Blazor WASM frontend.

1. Configure `PurpleExplorer.Api/appsettings.json` with your Service Bus connections (or Key Vault).
2. Run the API:
   - `dotnet run --project PurpleExplorer.Api/PurpleExplorer.Api.csproj`
3. Open `http://localhost:5209` (or the HTTPS URL shown in the console).

### Docker

Build and run:

1. `docker build -t purpleexplorer-web .`
2. `docker run -p 8080:8080 purpleexplorer-web`

Or use compose:

1. `docker compose up --build`

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
