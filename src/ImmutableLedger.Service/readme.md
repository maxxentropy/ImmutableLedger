# ImmutableLedger.Service

## Overview

ImmutableLedger.Service is the entry point and composition root of the Immutable Ledger system. It's a long-running service that processes ledger entries from a message queue, leveraging the Clean Architecture principles implemented across the other layers of the application.

## Purpose

The Service project serves several critical purposes:

1. Acts as the composition root for the application, setting up dependency injection.
2. Configures and starts the background service for processing messages.
3. Manages the application's lifecycle.
4. Integrates the different layers of the Clean Architecture.

## Key Components

### Program.cs

- The main entry point of the application.
- Sets up the host builder and configures services.

### LedgerWorker.cs

- Implements `IHostedService` to run as a background service.
- Manages the message processing loop.

### appsettings.json

- Contains configuration settings for the application, including database connections, AWS settings, and other environment-specific configurations.

## Configuration

The service is configured using the following:

1. **appsettings.json**: Contains default configuration values.
2. **Environment Variables**: Can override settings in appsettings.json.
3. **Command-line Arguments**: Can also be used to override configuration settings.

Key configuration sections include:

- AWS: Region and SQS settings
- ConnectionStrings: Database connection strings
- Logging: Log levels and providers

## Dependencies

This project depends on:

- ImmutableLedger.Domain
- ImmutableLedger.Application
- ImmutableLedger.Infrastructure
- Microsoft.Extensions.Hosting
- AWSSDK.Extensions.NETCore.Setup
- Other NuGet packages as required

## Running the Service

To run the service:

1. Ensure all configuration in appsettings.json is correctly set.
2. From the command line, navigate to the ImmutableLedger.Service directory.
3. Run `dotnet run` (or `dotnet run --environment Production` for production settings).

For development, you can also run the service from within Visual Studio or your preferred IDE.

## Deployment

The service can be deployed as a:

1. Windows Service
2. Linux Daemon
3. Docker Container
4. Kubernetes Pod

Ensure that the appropriate configuration and environment variables are set for the target environment.

## Monitoring and Logging

- The service uses the built-in .NET logging abstractions.
- Logs are configured in appsettings.json and can be directed to various sinks (console, file, cloud logging service).
- Consider integrating with application performance monitoring (APM) tools for production deployments.

## Extending the Service

When extending this service:

1. Add new hosted services by implementing `IHostedService` if new background processes are needed.
2. Update `Program.cs` to configure any new services or dependencies.
3. Modify `appsettings.json` to include any new configuration sections.

## Troubleshooting

Common issues and their solutions:

1. Service fails to start: Check configuration, ensure database is accessible, verify AWS credentials.
2. Messages not being processed: Verify SQS configuration, check network connectivity.
3. High CPU/Memory usage: Review message processing logic, consider scaling out the service.

## Best Practices

1. Keep the Service project focused on composition and configuration. Business logic should remain in the Application layer.
2. Regularly review and update NuGet packages to ensure security and performance.
3. Implement comprehensive logging and monitoring for production deployments.
4. Regularly test the service's resilience to network issues, database downtime, etc.