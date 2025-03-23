# FinalLab Test Suite

This repository contains comprehensive tests for the FinalLab application. The tests cover account and transaction operations, notifications, rollback (reverting) functionality, and edge cases. The tests are built using xUnit, Moq, and EF Core's In-Memory provider.

## Overview

The test suites cover the following areas:

- **Account & Transaction Tests:**  
  Validate account creation, deposit transactions, and withdrawal transactions, ensuring that balances update correctly, invalid amounts are rejected, and errors are returned for non-existent accounts.

- **Notification Tests:**  
  Ensure that notifications (update events) are created when accounts are modified or transactions occur. Tests verify notification retrieval by user ID and updating the notification's "read" status.

- **Rollback Tests:**  
  Implement new rollback handlers that revert events either by a specific day or for a specific account. Tests verify that events are correctly marked as reverted, filtering by account and transaction type works, and that rollbacks gracefully handle scenarios with no matching events.

- **Edge Case Tests:**  
  Validate system behavior when given invalid data (e.g., non-existent accounts, negative or zero transaction amounts) and test the behavior using maximum (`decimal.MaxValue`) and minimum positive transaction limits.

## Reverting Mechanism

The reverting (rollback) mechanism in FinalLab uses two key concepts:

1. **Domain Events with Reversion Flags:**  
   Each domain event (e.g., `AccountModifiedEvent` or `MoneyTransferredEvent`) contains an `IsReverting` flag that indicates whether the event is intended to reverse a previous change.  
   
2. **Update (Notification) Events with Recursive Relationships:**  
   Domain events are mapped to update events (e.g., `AccountUpdateEvent` and `TransactionUpdateEvent`). When reverting, the mapper swaps key fields (such as old and new statuses or balance changes) and links the new revert event to the original via a `ParentEventId`.  
   This creates an audit trail that records both the original change and its subsequent reversal.

## Project Structure

- **FinalLab.Tests/**  
  Contains all test suites:
  - **Account & Transaction Tests:** Validate normal operations and edge cases for deposits and withdrawals.
  - **Notification Tests:** Check creation, retrieval, and updating of notifications (update events).
  - **Rollback Tests:** Test two rollback scenarios:
    - Reverting all events on a specific day.
    - Reverting events for a specific account.
  - **Edge Case Tests:** Validate error handling and boundary conditions for transactions.

- **ApplicationDbContext:**  
  The existing EF Core DbContext used for integration testing with the In-Memory provider.

## Requirements

- [.NET Core](https://dotnet.microsoft.com/download) (or .NET 8)
- [xUnit](https://xunit.net/)
- [Moq](https://github.com/moq/moq4)
- [Microsoft.EntityFrameworkCore.InMemory](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.InMemory)

