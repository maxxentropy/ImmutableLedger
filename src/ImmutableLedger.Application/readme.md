# ImmutableLedger.Application

## Overview

The Application layer is a core part of the Clean Architecture in our Immutable Ledger system. It encapsulates and implements all use case logic, orchestrating the flow of data to and from the entities in the Domain layer, and directing those entities to use their critical business rules to achieve the goals of the use case.

## Purpose

This layer serves as an intermediary between the external world (UI, Web API, etc.) and the domain model. It's responsible for:

1. Defining the software's use cases
2. Orchestrating the flow of data to and from entities
3. Directing entities to use their business rules to achieve the goals of each use case

## Key Components

### Interfaces

- **ILedgerService**: Defines the contract for ledger operations at the application level.
- **IMessageHandler**: Specifies the contract for handling incoming messages from the message queue.

### Services

- **LedgerService**: Implements `ILedgerService`, providing the core application logic for ledger operations.
- **MessageHandler**: Implements `IMessageHandler`, processing incoming messages and coordinating with `LedgerService` to create new ledger entries.

### DTOs (Data Transfer Objects)

- **LedgerEntryMessage**: Represents the structure of incoming messages for new ledger entries.

## Design Principles

1. **Dependency Inversion**: This layer depends on abstractions (interfaces) rather than concretions, allowing for flexible and testable code.
2. **Single Responsibility**: Each service and handler is focused on a specific set of related use cases.
3. **Don't Repeat Yourself (DRY)**: Common logic is centralized in services to avoid duplication.
4. **Separation of Concerns**: The application logic is kept separate from both the domain logic and the infrastructure concerns.

## Interactions with Other Layers

- **Domain Layer**: The Application layer uses entities and interfaces defined in the Domain layer but doesn't alter them.
- **Infrastructure Layer**: It defines interfaces that are implemented by the Infrastructure layer, adhering to the Dependency Inversion Principle.
- **Presentation Layer**: While not directly referenced, this layer provides the use cases and application logic that the Presentation layer will utilize.

## Usage

The services and handlers in this layer should be used by the Presentation layer (or API endpoints) to execute business operations. They should be injected via Dependency Injection, using the interfaces defined here.

## Extending the Application Layer

When adding new functionality:

1. Define new interfaces in this layer if they represent application-specific abstractions.
2. Implement new services that orchestrate domain entities and infrastructure services to fulfill new use cases.
3. Ensure new components follow the existing patterns of dependency inversion and separation of concerns.

## Testing

The Application layer is well-suited for unit testing:

- Use mocking frameworks to create test doubles for the interfaces this layer depends on.
- Focus on testing the orchestration logic and use case flows.
- Ensure edge cases and error scenarios are thoroughly tested.

## Dependencies

This layer should have minimal external dependencies. It typically depends on:

- The Domain layer
- Basic .NET libraries
- Possibly some utility libraries for things like JSON serialization

It should not depend on UI, database, or any external agency frameworks.