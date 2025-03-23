# Dotnet Project: Task 1 – DDD & Core Functionality

## Overview

Task 1 focuses on establishing the foundation of the project by applying Domain-Driven Design (DDD) principles. This initial task sets up the core components across the domain, persistence, and application layers, providing a clear and scalable structure for subsequent tasks.

## Domain Layer

- **Entities:**
  - **Account:** Represents user or system accounts and serves as the basis for operations.
  - **Transaction:** Encapsulates the operations related to transactional activities.
  
These entities form the heart of the domain model, ensuring that business logic remains cleanly separated from infrastructural concerns.

## Persistence Layer

- **DbContext & DbContextFactory:**
  - A custom `DbContext` has been implemented to manage entity mappings and database interactions.
  - The `DbContextFactory` provides a standardized way to instantiate the context, promoting testability and a clear separation of concerns.
  
- **Repositories:**
  - Repositories have been set up to abstract data access. They serve as the interface between the domain and persistence layers, ensuring that the domain model is decoupled from the underlying database technology.

## Application Layer

- **Messaging with RabbitMQ:**
  - RabbitMQ has been integrated using MassTransit, enabling asynchronous communication between services.
  
- **Logging:**
  - Comprehensive logging has been established throughout the application using `ILogger`. This ensures that operations and potential errors are effectively tracked.

- **Transaction Service:**
  - A dedicated service is responsible for creating transactions in the database.
  - This service also publishes transaction events to RabbitMQ, supporting a scalable and reactive architecture.

## API Endpoints

- **POST /transaction-logs:**
  - Utilizes the Transaction Service to create new transactions in the database and to publish these events to RabbitMQ.
  
- **GET /transaction-logs/{accountId}:**
  - Retrieves transaction logs for a specific account using the configured DbContext.
  
- **GET /transaction-logs:**
  - Provides query capabilities using ODATA, enabling flexible and powerful querying of transaction logs.

## Conclusion

By leveraging DDD principles, this task establishes a robust, scalable foundation. The separation between domain logic, data persistence, and application concerns ensures that the project is well-structured for future development and enhancements.
