## Task 3 – Logging Microservice Integration

### Overview

Task 3 introduces a dedicated logging microservice within the infrastructure layer. This microservice is responsible for persisting log entries independently, enhancing separation of concerns and scalability for logging operations.

### Logging Microservice

- **Dedicated Logger:**
  - A new microservice has been added specifically for logging.
  - It maintains its own database, ensuring that log data is stored separately from the main application data.
  - The `LogEntry` entity is shared between the main application and the microservice to maintain a consistent log structure.

### Main Application Middleware

- **Request Logging Middleware:**
  - A new middleware component intercepts each request in the main application.
  - It creates a `LogEntry` object that captures details about the current request.
  - Using MassTransit and RabbitMQ, the middleware publishes the `LogEntry` to a message queue for further processing.

- **Data Collection & Forwarding:**
  - Within the middleware, a receive mechanism collects the published log entries.
  - The collected logs are then forwarded to the logging microservice via an HTTP POST request to the `/api/logs` endpoint, which handles saving the log entry in the microservice's database.

### Logging Microservice Endpoints

- **POST /api/logs:**
  - Receives log entries sent from the main application's middleware.
  - Saves each log entry to the microservice's database.

- **GET /api/logs:**
  - Retrieves stored log entries, providing visibility into the application's logging history.

### Conclusion

By decoupling logging into a separate microservice, Task 3 not only improves the system's maintainability and scalability but also ensures that logging operations do not impact the performance of the main application. The use of MassTransit, RabbitMQ, and dedicated HTTP endpoints provides a robust, asynchronous mechanism for processing and persisting logs.
