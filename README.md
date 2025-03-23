## Task 4 – Dispatching and Saving Events

### Overview

Task 4 introduces a robust event-driven architecture to the project. The main goals are to dispatch domain events, persist them as update events, and implement a system that supports event reversion.

### Event Classes

- **UpdateEvents:**
  - Represent the persisted depiction of events in the database.
  - Capture and store the state changes in a format suitable for long-term storage.

- **DomainEvents:**
  - Represent the events that are actively dispatched and handled in real-time.
  - Serve as the triggers for corresponding actions within the application.

### Infrastructure and Mapping

- **Event Mapper:**
  - A mapper in the infrastructure layer converts a DomainEvent into an UpdateEvent.
  - This mapping facilitates the saving of events in the database, ensuring consistency between dispatched events and their persisted counterparts.

### Event Handling and Reversion System

- **Domain Event Handlers:**
  - Created in the application layer, these handlers process every DomainEvent.
  - They incorporate logic to revert events when necessary, allowing the system to undo actions by referencing the parent event.

- **Event Reversion:**
  - The system supports reverting events. When an event is reverted, the UpdateEvent includes a reference to its parent event.
  - This design ensures that both the forward and reverse actions are traceable, enhancing auditability and error recovery.

### API Endpoints

- **POST /events:**
  - Accepts an event request DTO to post a specific DomainEvent.
  - Initiates the event handling workflow and ensures that the event is both dispatched and saved.

- **GET /events/{transactionID}:**
  - Retrieves all events related to a specific transaction.
  - Provides visibility into the event history, including any reversions, for auditing and debugging purposes.

### Conclusion

Task 4 significantly enhances the project's resilience and traceability by integrating a comprehensive event dispatching and persistence mechanism. By supporting event reversion, the system is well-equipped to handle corrections and maintain an accurate history of operations.
