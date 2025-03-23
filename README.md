## Task 2 – Query Implementation with MediatR and LINQ

### Overview

In Task 2, the focus was on expanding the functionality by introducing two key queries: **Get Common Transactions** and **Get Account Balance Summary**. These queries leverage the power of LINQ for data manipulation and MediatR for handling the requests, ensuring a clean separation between query definitions and their execution logic.

### Query Implementations

- **Get Common Transactions:**
  - Implemented to fetch and return transactions that are common or shared across multiple criteria.
  - The logic is encapsulated within a query handler in the application layer, making use of LINQ to filter and project the required data.

- **Get Account Balance Summary:**
  - Designed to compute and return a summary of the account balance for a specific user.
  - Uses a dedicated Balance DTO to encapsulate the result, ensuring that the data structure remains consistent and decoupled from the domain entities.
  - The query is handled in the application layer with LINQ, providing efficient aggregation and transformation of data.

### Application Layer Structure

- **Query Handlers:**
  - Both queries have been implemented in the application layer as part of their respective query handlers.
  - The handlers encapsulate the logic for processing the queries, ensuring that all operations related to querying the data are centralized.

- **MediatR Integration:**
  - MediatR is used to decouple the sending of queries from their handling, promoting a clean and maintainable codebase.
  - Endpoints simply dispatch the queries via MediatR, which routes them to the appropriate handler.

### API Endpoints

- **GET /accounts/common-transactions:**
  - This endpoint triggers the **Get Common Transactions** query.
  - It dispatches the query via MediatR and returns the list of common transactions as the result.

- **GET /accounts/balance-summary/{userID}:**
  - This endpoint is dedicated to the **Get Account Balance Summary** query.
  - It receives the user ID as a parameter, dispatches the query via MediatR, and returns the balance summary encapsulated in a Balance DTO.

### Conclusion

Task 2 builds on the solid foundation established in Task 1 by introducing advanced query capabilities using LINQ and MediatR. This modular approach not only improves the readability and maintainability of the code but also sets the stage for further enhancements and scalability in future tasks.
