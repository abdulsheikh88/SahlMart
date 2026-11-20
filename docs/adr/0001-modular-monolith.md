# ADR 0001: Start as a modular monolith

**Status:** Accepted

**Context:** The team is small and the domain (catalog, inventory, orders, dispatch) is still being learned. Microservices would add operational cost before it is needed.

**Decision:** Build one deployable ASP.NET Core application split into modules with explicit boundaries. Modules communicate through public interfaces and domain events, never through each other's database tables.

**Consequences:** Faster delivery and simpler operations. Inventory, Orders, Dispatch and Search can be extracted into services later when load requires it.
