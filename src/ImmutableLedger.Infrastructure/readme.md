# ImmutableLedger.Infrastructure

## Overview

The Infrastructure layer implements the concrete details of the Immutable Ledger system. It provides implementations for interfaces defined in the Domain and Application layers and manages interactions with external systems and services, including databases and message queues.

## Key Components

### Data Access

- **ShardedPostgreSqlLedgerRepository**: Implements the `ILedgerRepository` interface using PostgreSQL. It supports sharding for improved scalability and leverages read replicas for optimized query performance.

### Sharding and Read Replica Management

- **AdaptiveShardingStrategy**: Implements a dynamic sharding strategy that distributes data across shards based on region and account information.
- **ShardManager**: Manages the mapping between shard keys and database configurations. It handles both primary database instances and their associated read replicas.
- **DatabaseConfig**: Represents the configuration for a database shard, including connection strings for both the primary instance and its read replicas.

### Messaging

- **SqsClient**: Integrates with Amazon SQS for reliable message processing, handling message reception and deletion.

## Key Features

### Sharding Support

The infrastructure implements a sharding strategy to distribute data across multiple database instances, significantly improving scalability and performance. The adaptive nature of the strategy allows it to adjust to changing data patterns, maintaining an even distribution of data.

### Read Replica Support

Basic support for database read replicas:

1. **Performance Optimization**: By directing read operations to replicas, we reduce the load on primary database instances, optimizing overall system performance.
2. **Scalability**: Read replicas allow us to scale out read capacity independently of write capacity, accommodating read-heavy workloads efficiently.
3. **Flexibility**: Each shard can have multiple read replicas, and the system can dynamically choose which replica to use for each query.
4. **Fault Tolerance**: In case of issues with a primary instance, read operations can still be served by replicas, enhancing system resilience.

The `ShardManager` and `ShardedPostgreSqlLedgerRepository` work in tandem to integrate read replica support into data access operations.

### Cloud Integration

We leverage AWS services, specifically SQS for messaging, to enhance scalability and reliability.

## Configuration

The Infrastructure layer requires configuration for:
- Database connections (primary and read replicas)
- Sharding strategy
- AWS services

These configurations are typically provided through dependency injection and should be set up in the composition root of the application.

## Dependencies

- Npgsql: For PostgreSQL database access
- AWSSDK.SQS: For integration with Amazon SQS
- Microsoft.Extensions.Configuration: For accessing application configuration

## Usage

Components in this layer should not be used directly by the presentation layer. They are injected into Application layer services through dependency injection, adhering to the Dependency Inversion Principle.

## Extending the Infrastructure

When adding new infrastructure components:

1. Implement interfaces defined in the Domain or Application layers.
2. Use dependency injection for providing implementations to upper layers.
3. Keep external service integrations isolated within this layer.
4. Consider the impact on the sharding strategy and read replica configuration when adding new data access components.

## Testing

While unit testing is possible for some components, the Infrastructure layer often requires integration tests that interact with actual databases or external services. Consider using Docker for setting up test databases and mock AWS services for comprehensive testing. Ensure to include scenarios that test the read replica functionality and failover mechanisms.